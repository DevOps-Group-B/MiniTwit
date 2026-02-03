using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.Pages.Models;

public class BioModel
{
    [Required]
    public required string? RouteName { get; set; }
    [Required]
    public required bool IsMyBio { get; set; }
    public BioTextModel BioText { get; set; } = new BioTextModel();
}