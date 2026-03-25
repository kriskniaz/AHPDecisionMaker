using System.ComponentModel.DataAnnotations;

namespace AHPDecisionMaker.Web.ViewModels;

public class ProjectListItemViewModel
{
    public int ProjectId { get; set; }
    public string Goal { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool HasModel { get; set; }
    public int? ModelId { get; set; }
}

public class ProjectCreateEditViewModel
{
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Project goal is required.")]
    [MaxLength(200, ErrorMessage = "Goal cannot exceed 200 characters.")]
    [Display(Name = "Project Goal")]
    public string Goal { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public int? ModelId { get; set; }
}
