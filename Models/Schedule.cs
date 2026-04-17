using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class Schedule
{
    public int Id { get; set; }

    [Display(Name = "Gün")]
    public int DayId { get; set; }
    public Day? Day { get; set; }

    [Display(Name = "Saat")]
    public int TimeSlotId { get; set; }
    public TimeSlot? TimeSlot { get; set; }

    [Display(Name = "Derslik")]
    public int ClassroomId { get; set; }
    public Classroom? Classroom { get; set; }

    [Display(Name = "Sınıf")]
    public int ClassModelId { get; set; }
    public ClassModel? ClassModel { get; set; }

    [Display(Name = "Ders")]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Display(Name = "Hoca")]
    public int InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
}
