using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class ClassModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Sınıf adı zorunludur.")]
    [StringLength(10)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
