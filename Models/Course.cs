using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Hoca")]
    public int InstructorId { get; set; }
    public Instructor? Instructor { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
