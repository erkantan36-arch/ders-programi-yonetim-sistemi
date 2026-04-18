using ders_programi_yonetim_sistemi.Models;

namespace ders_programi_yonetim_sistemi.ViewModels;

public class HomeDashboardViewModel
{
    public int TotalCourses { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSchedules { get; set; }
    public List<Schedule> UpcomingSchedules { get; set; } = new();
}
