using ders_programi_yonetim_sistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Models.ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<Models.ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UserCount = await _userManager.Users.CountAsync();
        ViewBag.CourseCount = await _context.Courses.CountAsync();
        ViewBag.ScheduleCount = await _context.Schedules.CountAsync();
        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.Include(u => u.Instructor).OrderBy(u => u.UserName).ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await _context.Courses.Include(c => c.Instructor).OrderBy(c => c.Name).ToListAsync();
        return View(courses);
    }
}
