using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_MVC_POE_P2.Filters;
using PROG_MVC_POE_P2.Helpers;
using PROG_MVC_POE_P2.Data.Models;
using PROG_MVC_POE_P2.Services;

namespace PROG_MVC_POE_P2.Models;

[AuthorizeRole("HR")]
public class HRController : Controller
{
    private readonly ClaimsDbContext _context;
    public HRController(ClaimsDbContext context) => _context = context;

    public IActionResult Index()
    {
        var users = _context.Lecturers.OrderBy(l => l.Name).ToList();
        return View(users);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lecturer model, string password)
    {
        if (!ModelState.IsValid) return View(model);

        var (hash, salt) = PasswordHelper.HashPassword(password);
        model.PasswordHash = hash;
        model.PasswordSalt = salt;
        model.Role = model.Role ?? "Lecturer";

        _context.Lecturers.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var user = _context.Lecturers.Find(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Lecturer model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Lecturers.FindAsync(model.LecturerId);
        if (user == null) return NotFound();

        user.Name = model.Name;
        user.Email = model.Email;
        user.Faculty = model.Faculty;
        user.Position = model.Position;
        user.HourlyRate = model.HourlyRate;
        user.Role = model.Role;

        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Lecturers.FindAsync(id);
        if (user == null) return NotFound();
        _context.Lecturers.Remove(user);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "User removed.";
        return RedirectToAction(nameof(Index));
    }

    // Reset password action (HR can reset)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        var user = await _context.Lecturers.FindAsync(id);
        if (user == null) return NotFound();
        var (hash, salt) = PasswordHelper.HashPassword(newPassword);
        user.PasswordHash = hash; user.PasswordSalt = salt;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Password reset.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // Generate PDF report button (calls PdfService)
    public IActionResult Reports() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GenerateInvoiceForLecturer(int lecturerId)
    {
        var lecturer = _context.Lecturers.Find(lecturerId);
        if (lecturer == null) return NotFound();
        var claims = _context.Claims
            .Include(c => c.Pay)
            .Where(c => c.LecturerId == lecturerId && c.Status == "Approved")
            .ToList();

        var pdfBytes = HttpContext.RequestServices.GetService(typeof(IPdfService)) is IPdfService pdfService
            ? pdfService.GenerateInvoicePdf(lecturer, claims)
            : null;

        if (pdfBytes == null) return BadRequest("PDF service not available");

        return File(pdfBytes, "application/pdf", $"Invoice_{lecturer.Name}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
