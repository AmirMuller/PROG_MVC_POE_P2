using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_MVC_POE_P2.Filters;
using PROG_MVC_POE_P2.Data.Models;

namespace PROG_MVC_POE_P2.Controllers;

[AuthorizeRole("Lecturer", "HR", "Admin")]
public class ClaimsController : Controller
{
    private readonly ClaimsDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ClaimsController(ClaimsDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Create()
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var lecturer = _context.Lecturers.Find(userId);

        ViewBag.Lecturer = lecturer;
        return View();
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Login", "Account");

        var claims = await _context.Claims
            .Where(c => c.LecturerId == userId)
            .OrderByDescending(c => c.ClaimTime)
            .ToListAsync();

        return View(claims);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int numHours, IFormFile? uploadedFile, string? message)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
            return RedirectToAction("Login", "Account");

        var lecturer = await _context.Lecturers.FindAsync(userId.Value);
        if (lecturer == null)
            return NotFound();

        // Validate hours
        if (numHours <= 0 || numHours > 180)
        {
            TempData["ErrorMessage"] = "Hours must be between 1 and 180.";
            return RedirectToAction("Create");
        }

        // Validate hourly rate
        if (lecturer.HourlyRate <= 0)
        {
            TempData["ErrorMessage"] = "Hourly rate not set by HR.";
            return RedirectToAction("Create");
        }

        // Create payment
        var payment = new Payment
        {
            NumHours = numHours,
            Rate = lecturer.HourlyRate
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Create claim
        var claim = new Claim
        {
            LecturerId = lecturer.LecturerId,
            PayId = payment.PayId,
            ClaimTime = DateTime.Now,
            Status = "Pending",
            Message = message
        };

        // Handle file upload
        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var filename = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
            var filePath = Path.Combine(folder, filename);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fs);
            }

            claim.FilePath = "/uploads/" + filename;
        }

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Claim submitted.";
        return RedirectToAction(nameof(Index));
    }
}
