using ders_programi_yonetim_sistemi.Data;
using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Controllers;

[Authorize(Roles = "Admin")]
public class ClassroomController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClassroomController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var classrooms = await _context.Classrooms.OrderBy(c => c.Name).ToListAsync();
        return View(classrooms);
    }

    public IActionResult Create()
    {
        return View(new Classroom());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Classroom classroom)
    {
        if (ModelState.IsValid)
        {
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{classroom.Name}' dersliği eklendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(classroom);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var classroom = await _context.Classrooms.FindAsync(id);
        if (classroom == null) return NotFound();
        return View(classroom);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Classroom classroom)
    {
        if (id != classroom.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(classroom);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{classroom.Name}' dersliği güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(classroom);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var classroom = await _context.Classrooms.FindAsync(id);
        if (classroom == null) return NotFound();
        return View(classroom);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var classroom = await _context.Classrooms.Include(c => c.Schedules).FirstOrDefaultAsync(c => c.Id == id);
        if (classroom == null) return NotFound();

        if (classroom.Schedules.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "Bu dersliğe atanmış ders programları var. Önce onları silmelisiniz.");
            return View(classroom);
        }

        _context.Classrooms.Remove(classroom);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"'{classroom.Name}' dersliği silindi.";
        return RedirectToAction(nameof(Index));
    }
}
