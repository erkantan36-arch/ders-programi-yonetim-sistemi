using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Classroom
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
