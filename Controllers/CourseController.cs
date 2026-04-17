using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize(Roles = "Admin,Instructor")]
public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var query = _context.Courses.Include(c => c.Instructor).AsQueryable();

        if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
        {
            var instructorId = (await _userManager.GetUserAsync(User))?.InstructorId;
            if (instructorId.HasValue)
            {
                query = query.Where(c => c.InstructorId == instructorId.Value);
            }
            else
            {
                query = query.Where(c => false);
            }
        }

        return View(await query.OrderBy(c => c.Name).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await PopulateInstructorsAsync();
        return View(new Course());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        await EnforceInstructorOwnershipAsync(course);

        if (ModelState.IsValid)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateInstructorsAsync(course.InstructorId);
        return View(course);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(course))
        {
            return Forbid();
        }

        await PopulateInstructorsAsync(course.InstructorId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (existingCourse == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(existingCourse))
        {
            return Forbid();
        }

        await EnforceInstructorOwnershipAsync(course);

        if (ModelState.IsValid)
        {
            _context.Update(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateInstructorsAsync(course.InstructorId);
        return View(course);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(course))
        {
            return Forbid();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(course))
        {
            return Forbid();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateInstructorsAsync(int? selectedInstructorId = null)
    {
        var instructors = _context.Instructors.AsQueryable();

        if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
        {
            var instructorId = (await _userManager.GetUserAsync(User))?.InstructorId;
            if (instructorId.HasValue)
            {
                instructors = instructors.Where(i => i.Id == instructorId.Value);
                selectedInstructorId = instructorId;
            }
        }

        ViewData["InstructorId"] = new SelectList(await instructors.OrderBy(i => i.FullName).ToListAsync(), "Id", "FullName", selectedInstructorId);
    }

    private async Task EnforceInstructorOwnershipAsync(Course course)
    {
        if (User.IsInRole("Instructor") && !User.IsInRole("Admin"))
        {
            var instructorId = (await _userManager.GetUserAsync(User))?.InstructorId;
            if (!instructorId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bir hocaya bağlı değil.");
                return;
            }

            course.InstructorId = instructorId.Value;
        }
    }

    private async Task<bool> CanModifyCourseAsync(Course course)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (!User.IsInRole("Instructor"))
        {
            return false;
        }

        var instructorId = (await _userManager.GetUserAsync(User))?.InstructorId;
        return instructorId.HasValue && course.InstructorId == instructorId.Value;
    }
}
