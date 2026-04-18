using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Classroom
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Derslik adı zorunludur.")]
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;

    [Range(10, 500, ErrorMessage = "Kapasite 10-500 arasında olmalıdır.")]
    public int Capacity { get; set; } = 40;

    public bool IsAmphi { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
