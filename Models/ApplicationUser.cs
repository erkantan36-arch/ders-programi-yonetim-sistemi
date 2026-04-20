using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(150)]
    public string? FullName { get; set; }

    public int? InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
}
