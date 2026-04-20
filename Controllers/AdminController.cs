using ders_programi_yonetim_sistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        ViewBag.ClassroomCount = await _context.Classrooms.CountAsync();
        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.Include(u => u.Instructor).OrderBy(u => u.UserName).ToListAsync();
        var instructors = await _context.Instructors.OrderBy(i => i.FullName).ToListAsync();
        ViewBag.Instructors = new SelectList(instructors, "Id", "FullName");
        ViewBag.Roles = new[] { "Admin", "Instructor", "Viewer" };

        // Build a roles dictionary to avoid async calls in view
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var user in users)
        {
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }
        ViewBag.UserRoles = userRoles;

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignInstructor(string userId, int? instructorId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.InstructorId = instructorId;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"'{user.UserName}' için hoca ataması güncellendi.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string role)
    {
        var allowedRoles = new[] { "Admin", "Instructor", "Viewer" };
        if (!allowedRoles.Contains(role)) return BadRequest();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = $"'{user.UserName}' kullanıcısının rolü '{role}' olarak güncellendi.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await _context.Courses.Include(c => c.Instructor).OrderBy(c => c.Name).ToListAsync();
        return View(courses);
    }
}
