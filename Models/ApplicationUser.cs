using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(60)]
    public string? Title { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }
}
