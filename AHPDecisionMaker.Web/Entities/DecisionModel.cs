using System.ComponentModel.DataAnnotations;

namespace AHPDecisionMaker.Web.Entities;

public class DecisionModel
{
    public int ModelId { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [MaxLength(200)]
    public string? Goal { get; set; }

    public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
    public ICollection<Alternative> Alternatives { get; set; } = new List<Alternative>();
    public ICollection<CriteriaPairwiseComparison> CriteriaComparisons { get; set; } = new List<CriteriaPairwiseComparison>();
    public ICollection<AlternativePairwiseComparison> AlternativeComparisons { get; set; } = new List<AlternativePairwiseComparison>();
}
