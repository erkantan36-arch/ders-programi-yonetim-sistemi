using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize(Roles = "Admin,Instructor")]
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
        if (User.IsInRole("Admin"))
        {
            return View(await _context.Instructors.OrderBy(i => i.FullName).ToListAsync());
        }

        var instructorId = (await _userManager.GetUserAsync(User))?.InstructorId;
        var own = await _context.Instructors.Where(i => i.Id == instructorId).ToListAsync();
        return View(own);
    }

    public async Task<IActionResult> MySchedule()
    {
        int? instructorId;

        if (User.IsInRole("Admin"))
        {
            instructorId = await _context.Instructors.Select(i => (int?)i.Id).FirstOrDefaultAsync();
        }
        else
        {
            instructorId = (await _userManager.GetUserAsync(User))?.InstructorId;
        }

        if (!instructorId.HasValue)
        {
            return View(new List<Schedule>());
        }

        var schedules = await _context.Schedules
            .Include(s => s.Day)
            .Include(s => s.TimeSlot)
            .Include(s => s.Classroom)
            .Include(s => s.ClassModel)
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .Where(s => s.InstructorId == instructorId.Value)
            .OrderBy(s => s.DayId)
            .ThenBy(s => s.TimeSlotId)
            .ToListAsync();

        return View(schedules);
    }
}
