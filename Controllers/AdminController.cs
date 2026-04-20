using ders_programi_yonetim_sistemi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        if (instructorId.HasValue)
        {
            var instructor = await _context.Instructors.FindAsync(instructorId.Value);
            if (instructor != null && string.IsNullOrEmpty(instructor.UserId))
            {
                instructor.UserId = user.Id;
                await _context.SaveChangesAsync();
            }
        }

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

        if (role == "Instructor")
        {
            var instructor = await EnsureInstructorForUserAsync(user);
            if (user.InstructorId != instructor.Id)
            {
                user.InstructorId = instructor.Id;
                await _userManager.UpdateAsync(user);
            }
        }

        TempData["Success"] = $"'{user.UserName}' kullanıcısının rolü '{role}' olarak güncellendi.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await _context.Courses.Include(c => c.Instructor).OrderBy(c => c.Name).ToListAsync();
        return View(courses);
    }

    private async Task<Instructor> EnsureInstructorForUserAsync(ApplicationUser user)
    {
        if (user.InstructorId.HasValue)
        {
            var linkedInstructor = await _context.Instructors.FindAsync(user.InstructorId.Value);
            if (linkedInstructor != null)
            {
                if (linkedInstructor.UserId != user.Id)
                {
                    linkedInstructor.UserId = user.Id;
                    await _context.SaveChangesAsync();
                }
                return linkedInstructor;
            }
        }

        var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == user.Id);
        if (instructor != null)
        {
            return instructor;
        }

        instructor = new Instructor
        {
            FullName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.UserName ?? user.Email ?? "Yeni Eğitici",
            UserId = user.Id
        };

        _context.Instructors.Add(instructor);
        await _context.SaveChangesAsync();
        return instructor;
    }
}
