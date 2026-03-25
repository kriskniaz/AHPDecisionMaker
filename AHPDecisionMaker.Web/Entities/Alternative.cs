using System.ComponentModel.DataAnnotations;

namespace AHPDecisionMaker.Web.Entities;

public class Alternative
{
    public int AlternativeId { get; set; }

    public int ModelId { get; set; }
    public DecisionModel Model { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int OrderIndex { get; set; }
}
