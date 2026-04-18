using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Schedule
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Gün seçimi zorunludur.")]
    public int DayId { get; set; }

    [Required(ErrorMessage = "Saat seçimi zorunludur.")]
    public int TimeSlotId { get; set; }

    [Required(ErrorMessage = "Derslik seçimi zorunludur.")]
    public int ClassroomId { get; set; }

    [Required(ErrorMessage = "Sınıf seçimi zorunludur.")]
    public int ClassModelId { get; set; }

    [Required(ErrorMessage = "Ders seçimi zorunludur.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Hoca seçimi zorunludur.")]
    public int InstructorId { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    public Day Day { get; set; } = default!;
    public TimeSlot TimeSlot { get; set; } = default!;
    public Classroom Classroom { get; set; } = default!;
    public ClassModel ClassModel { get; set; } = default!;
    public Course Course { get; set; } = default!;
    public Instructor Instructor { get; set; } = default!;
}
