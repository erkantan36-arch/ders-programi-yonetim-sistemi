using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ders adı zorunludur.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ders kodu zorunludur.")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Kredi 1-10 arasında olmalıdır.")]
    public int Credit { get; set; } = 2;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
