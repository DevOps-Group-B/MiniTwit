using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.Pages.Models;

public class BioTextModel
{
    [StringLength(300)]
    public string? Bio { get; set; }
}