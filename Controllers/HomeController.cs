using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using ders_programi_yonetim_sistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeDashboardViewModel
        {
            TotalCourses = await _context.Courses.CountAsync(),
            TotalInstructors = await _context.Instructors.CountAsync(),
            TotalClasses = await _context.Classes.CountAsync(),
            TotalSchedules = await _context.Schedules.CountAsync(),
            UpcomingSchedules = await _context.Schedules
                .Include(x => x.Day)
                .Include(x => x.TimeSlot)
                .Include(x => x.ClassModel)
                .Include(x => x.Course)
                .Include(x => x.Instructor)
                .OrderBy(x => x.Day.Order)
                .ThenBy(x => x.TimeSlot.StartTime)
                .Take(10)
                .ToListAsync()
        };

        return View(vm);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
