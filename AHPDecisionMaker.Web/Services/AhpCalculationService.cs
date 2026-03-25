using AHPDecisionMaker.Web.Entities;
using AHPDecisionMaker.Web.ViewModels;
using Net.Kniaz.AHP;

namespace AHPDecisionMaker.Web.Services;

public class AhpCalculationService : IAhpCalculationService
{
    public AhpResultDto Calculate(DecisionModel model)
    {
        var result = new AhpResultDto();

        var criteria = model.Criteria.OrderBy(c => c.OrderIndex).ToList();
        var alternatives = model.Alternatives.OrderBy(a => a.OrderIndex).ToList();

        if (criteria.Count < 2)
        {
            result.Success = false;
            result.ErrorMessage = "At least 2 criteria are required to perform AHP calculations.";
            return result;
        }
        if (alternatives.Count < 2)
        {
            result.Success = false;
            result.ErrorMessage = "At least 2 alternatives are required to perform AHP calculations.";
            return result;
        }

        try
        {
            // Build AHPObjectModel
            var ahpModel = new AHPObjectModel(model.Goal ?? "Decision");

            ahpModel.Criteria = criteria
                .Select(c => new Net.Kniaz.AHP.Criterion(c.CriterionId.ToString(), c.Name, string.Empty))
                .ToList();

            ahpModel.Alternatives = alternatives
                .Select(a => new Net.Kniaz.AHP.Alternative(a.AlternativeId.ToString(), a.Name, string.Empty))
                .ToList();

            // Set up criteria pairwise comparison matrix
            ahpModel.InitializeCriteriaComparisons();

            foreach (var comp in model.CriteriaComparisons)
            {
                ahpModel.CriteriaComparisons.SetComparison(
                    comp.CriterionAId.ToString(),
                    comp.CriterionBId.ToString(),
                    comp.Value);
            }

            // Calculate criteria weights
            var criteriaWeights = ahpModel.CriteriaComparisons.CalculatePriorityVector();
            var criteriaCR = ahpModel.CriteriaComparisons.CalculateConsistencyRatio();
            result.CriteriaConsistencyRatio = criteriaCR;

            foreach (var c in criteria)
            {
                criteriaWeights.TryGetValue(c.CriterionId.ToString(), out double weight);
                result.CriteriaWeights.Add(new CriterionWeightDto
                {
                    CriterionId = c.CriterionId,
                    Name = c.Name,
                    Weight = weight
                });
            }

            // Set up alternative comparison matrices per criterion
            ahpModel.InitializeAllAlternativeComparisons();

            foreach (var comp in model.AlternativeComparisons)
            {
                var criterionKey = comp.CriterionId.ToString();
                if (ahpModel.AlternativeComparisons.ContainsKey(criterionKey))
                {
                    ahpModel.AlternativeComparisons[criterionKey].SetComparison(
                        comp.AlternativeAId.ToString(),
                        comp.AlternativeBId.ToString(),
                        comp.Value);
                }
            }

            // Calculate scores per criterion and final scores
            var finalScores = new Dictionary<int, double>();
            foreach (var alt in alternatives)
                finalScores[alt.AlternativeId] = 0.0;

            foreach (var criterion in criteria)
            {
                var criterionKey = criterion.CriterionId.ToString();
                criteriaWeights.TryGetValue(criterionKey, out double criterionWeight);

                var altWeights = ahpModel.AlternativeComparisons[criterionKey].CalculatePriorityVector();
                var altCR = ahpModel.AlternativeComparisons[criterionKey].CalculateConsistencyRatio();

                // Update criterion weight with per-criterion CR
                var cw = result.CriteriaWeights.FirstOrDefault(cw => cw.CriterionId == criterion.CriterionId);
                if (cw != null) cw.ConsistencyRatio = altCR;

                foreach (var alt in alternatives)
                {
                    altWeights.TryGetValue(alt.AlternativeId.ToString(), out double altScore);
                    finalScores[alt.AlternativeId] += criterionWeight * altScore;

                    result.DetailScores.Add(new AlternativeCriterionScoreDto
                    {
                        AlternativeId = alt.AlternativeId,
                        AlternativeName = alt.Name,
                        CriterionId = criterion.CriterionId,
                        CriterionName = criterion.Name,
                        Score = altScore
                    });
                }
            }

            // Build ranked results
            var ranked = alternatives
                .Select(a => new AlternativeResultDto
                {
                    AlternativeId = a.AlternativeId,
                    Name = a.Name,
                    FinalScore = finalScores[a.AlternativeId]
                })
                .OrderByDescending(a => a.FinalScore)
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
                ranked[i].Rank = i + 1;

            result.AlternativeResults = ranked;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Calculation error: {ex.Message}";
        }

        return result;
    }
}
