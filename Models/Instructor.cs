using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Instructor
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
