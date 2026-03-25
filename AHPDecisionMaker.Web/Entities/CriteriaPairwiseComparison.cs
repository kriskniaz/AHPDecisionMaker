namespace AHPDecisionMaker.Web.Entities;

public class CriteriaPairwiseComparison
{
    public int Id { get; set; }

    public int ModelId { get; set; }
    public DecisionModel Model { get; set; } = null!;

    public int CriterionAId { get; set; }
    public Criterion CriterionA { get; set; } = null!;

    public int CriterionBId { get; set; }
    public Criterion CriterionB { get; set; } = null!;

    /// <summary>
    /// AHP pairwise comparison value. A value > 1 means CriterionA is more important.
    /// Value of 1 = equal importance. Range: 1/9 to 9.
    /// </summary>
    public double Value { get; set; } = 1.0;
}
