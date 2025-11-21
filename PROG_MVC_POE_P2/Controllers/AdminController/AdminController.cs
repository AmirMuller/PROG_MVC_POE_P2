using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_MVC_POE_P2.Data.Models;
using PROG_MVC_POE_P2.Filters;
using PROG_MVC_POE_P2.Models;

[AuthorizeRole("Admin", "HR")]
public class AdminController : Controller
{
    private readonly ClaimsDbContext _context;
    public AdminController(ClaimsDbContext context) => _context = context;

    public IActionResult Index()
    {
        var reviewList = _context.Claims
            .Include(c => c.Pay)
            .Include(c => c.Lecturer)
            .OrderBy(c => c.Status)
            .ThenByDescending(c => c.ClaimTime)
            .Select(c => new ClaimReviewView
            {
                Claim = c,
                Payment = c.Pay, 
                LecturerName = c.Lecturer.Name,
                TotalAmount = c.Pay != null ? (c.Pay.NumHours * c.Pay.Rate) : 0
            })
            .ToList();

        return View(reviewList);
    }

    public IActionResult Review(int id)
    {
        var claim = _context.Claims
            .Include(c => c.Pay)
            .Include(c => c.Lecturer)
            .FirstOrDefault(c => c.ClaimId == id);

        if (claim == null)
            return NotFound();

        var vm = new ClaimReviewView
        {
            Claim = claim,
            Payment = claim.Pay,  // can be null safely
            LecturerName = claim.Lecturer.Name,
            TotalAmount = claim.Pay != null ? (claim.Pay.NumHours * claim.Pay.Rate) : 0
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessReview(int claimId, string action, string adminComment)
    {
        var claim = _context.Claims.FirstOrDefault(c => c.ClaimId == claimId);

        if (claim == null)
        {
            TempData["ErrorMessage"] = "Claim not found.";
            return RedirectToAction("Index");
        }

        if (claim.Status != "Pending")
        {
            TempData["ErrorMessage"] = "Claim already processed.";
            return RedirectToAction("Index");
        }

        if (action == "Approve")
        {
            claim.Status = "Approved";
        }
        else if (action == "Reject")
        {
            claim.Status = "Rejected";

            // ensure Message isn't null before appending
            if (string.IsNullOrWhiteSpace(claim.Message))
                claim.Message = "";

            claim.Message += $"\n(REJECTED - Admin: {adminComment})";
        }
        else
        {
            TempData["ErrorMessage"] = "Unknown action.";
            return RedirectToAction("Index");
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Review processed.";
        return RedirectToAction("Index");
    }
}
