using System.ComponentModel.DataAnnotations;

namespace AHPDecisionMaker.Web.ViewModels;

public class ModelEditViewModel
{
    public int ModelId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectGoal { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Decision Goal")]
    public string? Goal { get; set; }

    public List<CriterionItemViewModel> Criteria { get; set; } = new();
    public List<AlternativeItemViewModel> Alternatives { get; set; } = new();
    public List<ComparisonPairViewModel> CriteriaComparisons { get; set; } = new();
    public List<AlternativeComparisonGroupViewModel> AlternativeComparisonGroups { get; set; } = new();
    public AhpResultDto? CalculationResult { get; set; }
}

public class CriterionItemViewModel
{
    public int CriterionId { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class AlternativeItemViewModel
{
    public int AlternativeId { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class ComparisonPairViewModel
{
    public int Id { get; set; }
    public int CriterionAId { get; set; }
    public string CriterionAName { get; set; } = string.Empty;
    public int CriterionBId { get; set; }
    public string CriterionBName { get; set; } = string.Empty;
    public double Value { get; set; } = 1.0;
}

public class AlternativeComparisonGroupViewModel
{
    public int CriterionId { get; set; }
    public string CriterionName { get; set; } = string.Empty;
    public List<AlternativeComparisonPairViewModel> Pairs { get; set; } = new();
}

public class AlternativeComparisonPairViewModel
{
    public int Id { get; set; }
    public int AlternativeAId { get; set; }
    public string AlternativeAName { get; set; } = string.Empty;
    public int AlternativeBId { get; set; }
    public string AlternativeBName { get; set; } = string.Empty;
    public double Value { get; set; } = 1.0;
}
