using System.ComponentModel.DataAnnotations;

namespace AHPDecisionMaker.Web.Entities;

public class Project
{
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Goal { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

    public DecisionModel? DecisionModel { get; set; }
}
