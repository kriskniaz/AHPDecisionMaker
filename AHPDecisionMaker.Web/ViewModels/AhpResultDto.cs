namespace AHPDecisionMaker.Web.ViewModels;

public class AhpResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public List<CriterionWeightDto> CriteriaWeights { get; set; } = new();
    public double CriteriaConsistencyRatio { get; set; }

    public List<AlternativeResultDto> AlternativeResults { get; set; } = new();

    // Per-criterion scores for each alternative
    public List<AlternativeCriterionScoreDto> DetailScores { get; set; } = new();
}

public class CriterionWeightDto
{
    public int CriterionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double ConsistencyRatio { get; set; }
}

public class AlternativeResultDto
{
    public int AlternativeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double FinalScore { get; set; }
    public int Rank { get; set; }
}

public class AlternativeCriterionScoreDto
{
    public int AlternativeId { get; set; }
    public string AlternativeName { get; set; } = string.Empty;
    public int CriterionId { get; set; }
    public string CriterionName { get; set; } = string.Empty;
    public double Score { get; set; }
}
