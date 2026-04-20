using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ders_programi_yonetim_sistemi.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ScheduleCount = await _context.Schedules.CountAsync();
        ViewBag.CourseCount = await _context.Courses.CountAsync();
        ViewBag.InstructorCount = await _context.Instructors.CountAsync();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
