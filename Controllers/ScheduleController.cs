using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize(Roles = "Admin,Instructor,Viewer")]
public class ScheduleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ScheduleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var schedules = await _context.Schedules
            .Include(s => s.Day)
            .Include(s => s.TimeSlot)
            .Include(s => s.Classroom)
            .Include(s => s.ClassModel)
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .OrderBy(s => s.DayId)
            .ThenBy(s => s.TimeSlotId)
            .ThenBy(s => s.ClassModel!.Name)
            .ToListAsync();

        return View(schedules);
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropDownsAsync();
        return View(new Schedule());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(Schedule schedule)
    {
        await EnforceInstructorOwnershipAsync(schedule);

        if (await HasConflictAsync(schedule))
        {
            ModelState.AddModelError(string.Empty, "Çakışma var: aynı saatte derslik, sınıf veya hoca meşgul.");
        }

        if (!await CourseBelongsToInstructorAsync(schedule.CourseId, schedule.InstructorId))
        {
            ModelState.AddModelError(nameof(Schedule.CourseId), "Seçilen ders bu hocaya ait değil.");
        }

        if (ModelState.IsValid)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropDownsAsync(schedule.InstructorId);
        return View(schedule);
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        if (!await CanModifyScheduleAsync(schedule))
        {
            return Forbid();
        }

        await PopulateDropDownsAsync(schedule.InstructorId);
        return View(schedule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id, Schedule schedule)
    {
        if (id != schedule.Id)
        {
            return NotFound();
        }

        var existingSchedule = await _context.Schedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (existingSchedule == null)
        {
            return NotFound();
        }

        if (!await CanModifyScheduleAsync(existingSchedule))
        {
            return Forbid();
        }

        await EnforceInstructorOwnershipAsync(schedule);

        if (await HasConflictAsync(schedule))
        {
            ModelState.AddModelError(string.Empty, "Çakışma var: aynı saatte derslik, sınıf veya hoca meşgul.");
        }

        if (!await CourseBelongsToInstructorAsync(schedule.CourseId, schedule.InstructorId))
        {
            ModelState.AddModelError(nameof(Schedule.CourseId), "Seçilen ders bu hocaya ait değil.");
        }

        if (ModelState.IsValid)
        {
            _context.Update(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropDownsAsync(schedule.InstructorId);
        return View(schedule);
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Day)
            .Include(s => s.TimeSlot)
            .Include(s => s.Classroom)
            .Include(s => s.ClassModel)
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
        {
            return NotFound();
        }

        if (!await CanModifyScheduleAsync(schedule))
        {
            return Forbid();
        }

        return View(schedule);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        if (!await CanModifyScheduleAsync(schedule))
        {
            return Forbid();
        }

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropDownsAsync(int? selectedInstructorId = null)
    {
        var isInstructor = User.IsInRole("Instructor") && !User.IsInRole("Admin");
        int? currentInstructorId = await GetCurrentInstructorIdAsync();

        var instructors = _context.Instructors.AsQueryable();
        var courses = _context.Courses.Include(c => c.Instructor).AsQueryable();

        if (isInstructor && currentInstructorId.HasValue)
        {
            instructors = instructors.Where(i => i.Id == currentInstructorId.Value);
            courses = courses.Where(c => c.InstructorId == currentInstructorId.Value);
            selectedInstructorId = currentInstructorId;
        }

        ViewData["DayId"] = new SelectList(await _context.Days.OrderBy(d => d.Id).ToListAsync(), "Id", "Name");
        ViewData["TimeSlotId"] = new SelectList(await _context.TimeSlots.OrderBy(t => t.Id).ToListAsync(), "Id", "DisplayName");
        ViewData["ClassroomId"] = new SelectList(await _context.Classrooms.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewData["ClassModelId"] = new SelectList(await _context.Classes.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewData["CourseId"] = new SelectList(await courses.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewData["InstructorId"] = new SelectList(await instructors.OrderBy(i => i.FullName).ToListAsync(), "Id", "FullName", selectedInstructorId);
    }

    private async Task<int?> GetCurrentInstructorIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.InstructorId;
    }

    private async Task EnforceInstructorOwnershipAsync(Schedule schedule)
    {
        if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
        {
            var instructorId = await GetCurrentInstructorIdAsync();
            if (!instructorId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bir hocaya bağlı değil.");
                return;
            }

            schedule.InstructorId = instructorId.Value;
        }
    }

    private async Task<bool> CanModifyScheduleAsync(Schedule schedule)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (!User.IsInRole("Instructor"))
        {
            return false;
        }

        var instructorId = await GetCurrentInstructorIdAsync();
        return instructorId.HasValue && schedule.InstructorId == instructorId.Value;
    }

    private async Task<bool> CourseBelongsToInstructorAsync(int courseId, int instructorId)
    {
        return await _context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);
    }

    private async Task<bool> HasConflictAsync(Schedule schedule)
    {
        return await _context.Schedules.AnyAsync(s =>
            s.Id != schedule.Id &&
            s.DayId == schedule.DayId &&
            s.TimeSlotId == schedule.TimeSlotId &&
            (
                s.ClassroomId == schedule.ClassroomId ||
                s.ClassModelId == schedule.ClassModelId ||
                s.InstructorId == schedule.InstructorId
            ));
    }
}
