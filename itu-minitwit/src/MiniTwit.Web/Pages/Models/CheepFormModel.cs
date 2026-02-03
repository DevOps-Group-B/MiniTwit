using System.ComponentModel.DataAnnotations;
using Chirp.Core.DTOs;

namespace Chirp.Web.Pages.Models;

public class CheepFormModel
{
    public AuthorDTO Author { get; set; } = null!;
    public CheepFormTextModel FormData { get; set; } = null!;
}