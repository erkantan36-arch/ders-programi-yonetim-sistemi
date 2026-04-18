using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class TimeSlot
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeOnly EndTime { get; set; }

    [StringLength(30)]
    public string Label { get; set; } = string.Empty;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
