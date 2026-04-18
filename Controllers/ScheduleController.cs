using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using ders_programi_yonetim_sistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize]
public class ScheduleController : Controller
{
    private readonly ApplicationDbContext _context;

    public ScheduleController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? classId, int? instructorId, int? classroomId)
    {
        await PopulateLookups(classId, instructorId, classroomId);

        var query = _context.Schedules
            .Include(x => x.Day)
            .Include(x => x.TimeSlot)
            .Include(x => x.Classroom)
            .Include(x => x.ClassModel)
            .Include(x => x.Course)
            .Include(x => x.Instructor)
            .AsQueryable();

        if (classId.HasValue) query = query.Where(x => x.ClassModelId == classId.Value);
        if (instructorId.HasValue) query = query.Where(x => x.InstructorId == instructorId.Value);
        if (classroomId.HasValue) query = query.Where(x => x.ClassroomId == classroomId.Value);

        var schedules = await query
            .OrderBy(x => x.Day.Order)
            .ThenBy(x => x.TimeSlot.StartTime)
            .ToListAsync();

        return View(schedules);
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create()
    {
        await PopulateLookups();
        return View(new ScheduleFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(ScheduleFormViewModel model)
    {
        await EnforceInstructorScopeAsync(model);

        if (await HasConflictAsync(model))
        {
            ModelState.AddModelError(string.Empty, "Aynı gün ve saatte derslik, sınıf veya hoca çakışması var.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.ClassModelId, model.InstructorId, model.ClassroomId);
            return View(model);
        }

        var schedule = new Schedule
        {
            DayId = model.DayId,
            TimeSlotId = model.TimeSlotId,
            ClassroomId = model.ClassroomId,
            ClassModelId = model.ClassModelId,
            CourseId = model.CourseId,
            InstructorId = model.InstructorId,
            Notes = model.Notes
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule is null) return NotFound();

        if (await IsForbiddenForInstructorAsync(schedule.InstructorId)) return Forbid();

        await PopulateLookups(schedule.ClassModelId, schedule.InstructorId, schedule.ClassroomId);
        return View(new ScheduleFormViewModel
        {
            Id = schedule.Id,
            DayId = schedule.DayId,
            TimeSlotId = schedule.TimeSlotId,
            ClassroomId = schedule.ClassroomId,
            ClassModelId = schedule.ClassModelId,
            CourseId = schedule.CourseId,
            InstructorId = schedule.InstructorId,
            Notes = schedule.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id, ScheduleFormViewModel model)
    {
        if (id != model.Id) return NotFound();

        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule is null) return NotFound();

        if (await IsForbiddenForInstructorAsync(schedule.InstructorId)) return Forbid();

        await EnforceInstructorScopeAsync(model);

        if (await HasConflictAsync(model, id))
        {
            ModelState.AddModelError(string.Empty, "Aynı gün ve saatte derslik, sınıf veya hoca çakışması var.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.ClassModelId, model.InstructorId, model.ClassroomId);
            return View(model);
        }

        schedule.DayId = model.DayId;
        schedule.TimeSlotId = model.TimeSlotId;
        schedule.ClassroomId = model.ClassroomId;
        schedule.ClassModelId = model.ClassModelId;
        schedule.CourseId = model.CourseId;
        schedule.InstructorId = model.InstructorId;
        schedule.Notes = model.Notes;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var schedule = await _context.Schedules
            .Include(x => x.Day)
            .Include(x => x.TimeSlot)
            .Include(x => x.ClassModel)
            .Include(x => x.Course)
            .Include(x => x.Instructor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (schedule is null) return NotFound();
        if (await IsForbiddenForInstructorAsync(schedule.InstructorId)) return Forbid();

        return View(schedule);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule is null) return NotFound();
        if (await IsForbiddenForInstructorAsync(schedule.InstructorId)) return Forbid();

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CheckConflict(int dayId, int timeSlotId, int classroomId, int classModelId, int instructorId, int? excludeId)
    {
        var hasConflict = await _context.Schedules.AnyAsync(x =>
            x.DayId == dayId &&
            x.TimeSlotId == timeSlotId &&
            (x.ClassroomId == classroomId || x.ClassModelId == classModelId || x.InstructorId == instructorId) &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

        return Json(new { hasConflict });
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleForDay(int dayId)
    {
        var list = await _context.Schedules
            .Where(x => x.DayId == dayId)
            .Include(x => x.TimeSlot)
            .Include(x => x.Course)
            .Include(x => x.ClassModel)
            .Include(x => x.Classroom)
            .Include(x => x.Instructor)
            .OrderBy(x => x.TimeSlot.StartTime)
            .Select(x => new
            {
                x.Id,
                Time = x.TimeSlot.Label,
                Course = x.Course.Name,
                Class = x.ClassModel.Name,
                Classroom = x.Classroom.Name,
                Instructor = x.Instructor.FullName
            })
            .ToListAsync();

        return Json(list);
    }

    private async Task PopulateLookups(int? classId = null, int? instructorId = null, int? classroomId = null)
    {
        ViewBag.Days = new SelectList(await _context.Days.OrderBy(x => x.Order).ToListAsync(), "Id", "Name");
        ViewBag.TimeSlots = new SelectList(await _context.TimeSlots.OrderBy(x => x.StartTime).ToListAsync(), "Id", "Label");
        ViewBag.Classrooms = new SelectList(await _context.Classrooms.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", classroomId);
        ViewBag.Classes = new SelectList(await _context.Classes.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", classId);
        ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        ViewBag.Instructors = new SelectList(await _context.Instructors.OrderBy(x => x.FullName).ToListAsync(), "Id", "FullName", instructorId);
    }

    private async Task<bool> HasConflictAsync(ScheduleFormViewModel model, int? excludeId = null)
    {
        return await _context.Schedules.AnyAsync(x =>
            x.DayId == model.DayId &&
            x.TimeSlotId == model.TimeSlotId &&
            (x.ClassroomId == model.ClassroomId || x.ClassModelId == model.ClassModelId || x.InstructorId == model.InstructorId) &&
            (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    private async Task EnforceInstructorScopeAsync(ScheduleFormViewModel model)
    {
        if (!User.IsInRole("Instructor") || User.IsInRole("Admin")) return;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return;

        var instructor = await _context.Instructors.FirstOrDefaultAsync(x => x.ApplicationUserId == userId);
        if (instructor is not null)
        {
            model.InstructorId = instructor.Id;
        }
    }

    private async Task<bool> IsForbiddenForInstructorAsync(int instructorId)
    {
        if (!User.IsInRole("Instructor") || User.IsInRole("Admin")) return false;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return true;

        return !await _context.Instructors.AnyAsync(x => x.Id == instructorId && x.ApplicationUserId == userId);
    }
}
