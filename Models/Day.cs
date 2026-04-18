using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Day
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Gün adı zorunludur.")]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 7, ErrorMessage = "Sıra 1-7 arasında olmalıdır.")]
    public int Order { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
