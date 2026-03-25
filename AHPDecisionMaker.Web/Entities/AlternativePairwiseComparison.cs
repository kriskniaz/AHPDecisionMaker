namespace AHPDecisionMaker.Web.Entities;

public class AlternativePairwiseComparison
{
    public int Id { get; set; }

    public int ModelId { get; set; }
    public DecisionModel Model { get; set; } = null!;

    public int CriterionId { get; set; }
    public Criterion Criterion { get; set; } = null!;

    public int AlternativeAId { get; set; }
    public Alternative AlternativeA { get; set; } = null!;

    public int AlternativeBId { get; set; }
    public Alternative AlternativeB { get; set; } = null!;

    /// <summary>
    /// AHP pairwise comparison value for this criterion.
    /// A value > 1 means AlternativeA is preferred over AlternativeB.
    /// </summary>
    public double Value { get; set; } = 1.0;
}
