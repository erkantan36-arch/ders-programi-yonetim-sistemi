using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using ders_programi_yonetim_sistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize]
public class InstructorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Instructors.OrderBy(x => x.FullName).ToListAsync());
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> MySchedule()
    {
        var userId = _userManager.GetUserId(User);
        var instructor = await _context.Instructors.FirstOrDefaultAsync(x => x.ApplicationUserId == userId);
        if (instructor is null)
        {
            return View(new List<Schedule>());
        }

        var list = await _context.Schedules
            .Where(x => x.InstructorId == instructor.Id)
            .Include(x => x.Day)
            .Include(x => x.TimeSlot)
            .Include(x => x.Course)
            .Include(x => x.ClassModel)
            .Include(x => x.Classroom)
            .OrderBy(x => x.Day.Order)
            .ThenBy(x => x.TimeSlot.StartTime)
            .ToListAsync();

        return View(list);
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var vm = new ProfileViewModel
        {
            FullName = user.FullName,
            Title = user.Title,
            Department = user.Department,
            PhoneNumber = user.PhoneNumber
        };

        return View(vm);
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Title = user.Title,
            Department = user.Department,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> EditProfile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        user.FullName = model.FullName;
        user.Title = model.Title;
        user.Department = model.Department;
        user.PhoneNumber = model.PhoneNumber;

        await _userManager.UpdateAsync(user);

        var instructor = await _context.Instructors.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);
        if (instructor is not null)
        {
            instructor.FullName = model.FullName;
            instructor.Title = model.Title ?? instructor.Title;
            instructor.PhoneNumber = model.PhoneNumber;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(MyProfile));
    }
}
