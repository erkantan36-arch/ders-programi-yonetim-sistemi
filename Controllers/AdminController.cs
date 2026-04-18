using ders_programi_yonetim_sistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ders_programi_yonetim_sistemi.Models;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.TotalSchedules = await _context.Schedules.CountAsync();
        return View();
    }

    public async Task<IActionResult> ManageDays() => View(await _context.Days.OrderBy(x => x.Order).ToListAsync());
    public async Task<IActionResult> ManageClassrooms() => View(await _context.Classrooms.OrderBy(x => x.Name).ToListAsync());
    public async Task<IActionResult> ManageTimeSlots() => View(await _context.TimeSlots.OrderBy(x => x.StartTime).ToListAsync());
    public async Task<IActionResult> ManageClasses() => View(await _context.Classes.OrderBy(x => x.Name).ToListAsync());
    public async Task<IActionResult> ManageInstructors() => View(await _context.Instructors.OrderBy(x => x.FullName).ToListAsync());

    public async Task<IActionResult> ManageUsers()
    {
        var users = await _userManager.Users.OrderBy(x => x.Email).ToListAsync();
        var roles = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            roles[user.Id] = await _userManager.GetRolesAsync(user);
        }

        ViewBag.UserRoles = roles;
        return View(users);
    }
}
