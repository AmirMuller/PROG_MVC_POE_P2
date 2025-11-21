using Microsoft.AspNetCore.Mvc;
using PROG_MVC_POE_P2.Helpers;
using PROG_MVC_POE_P2.Data.Models;

namespace PROG_MVC_POE_P2.Controllers.AccountController;

public class AccountController : Controller
{
    private readonly ClaimsDbContext _context;

    public AccountController(ClaimsDbContext context)
    {
        _context = context;
    }

    public IActionResult Login(string returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password, string returnUrl = null)
    {
        // Find lecturer by email
        var lecturer = _context.Lecturers.FirstOrDefault(l => l.Email == email);

        if (lecturer == null)
        {
            TempData["ErrorMessage"] = "Invalid email or password.";
            return RedirectToAction("Login");
        }

        // Verify password
        bool validPassword =
            lecturer.PasswordSalt != null &&
            lecturer.PasswordHash != null &&
            PasswordHelper.VerifyPassword(password, lecturer.PasswordSalt, lecturer.PasswordHash);

        if (!validPassword)
        {
            TempData["ErrorMessage"] = "Invalid email or password.";
            return RedirectToAction("Login");
        }

        // Store session
        HttpContext.Session.SetInt32("UserId", lecturer.LecturerId);
        HttpContext.Session.SetString("UserRole", lecturer.Role);
        HttpContext.Session.SetString("UserName", lecturer.Name);

        // Redirect if returnUrl provided
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Redirect by role
        return lecturer.Role switch
        {
            "HR" => RedirectToAction("Index", "HR"),
            "Admin" => RedirectToAction("Index", "Admin"),
            _ => RedirectToAction("Index", "Claim")   // default for lecturers
        };
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}