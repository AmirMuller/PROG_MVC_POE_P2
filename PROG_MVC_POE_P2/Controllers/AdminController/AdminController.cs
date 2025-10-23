using Microsoft.AspNetCore.Mvc;
using PROG_MVC_POE_P2.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace PROG_MVC_POE_P2.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _claimsFilePath;
        private readonly string _paymentsFilePath;
        private readonly string _lecturersFilePath;
        private readonly IWebHostEnvironment _env;

        public AdminController(IWebHostEnvironment env)
        {
            _env = env;
            var dataPath = Path.Combine(_env.ContentRootPath, "Data");

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            _claimsFilePath = Path.Combine(dataPath, "claims.json");
            _paymentsFilePath = Path.Combine(dataPath, "payments.json");
            _lecturersFilePath = Path.Combine(dataPath, "lecturers.json");
        }

        //=======================================================
        //helper methods

        private List<Claim> LoadClaims()
        {
            if (!System.IO.File.Exists(_claimsFilePath)) return new List<Claim>();
            var json = System.IO.File.ReadAllText(_claimsFilePath);
            return JsonSerializer.Deserialize<List<Claim>>(json) ?? new List<Claim>();
        }

        private void SaveClaims(List<Claim> claims)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(claims, options);
            System.IO.File.WriteAllText(_claimsFilePath, json);
        }

        private List<Payment> LoadPayments()
        {
            if (!System.IO.File.Exists(_paymentsFilePath)) return new List<Payment>();
            var json = System.IO.File.ReadAllText(_paymentsFilePath);
            return JsonSerializer.Deserialize<List<Payment>>(json) ?? new List<Payment>();
        }

        private List<Lecturer> LoadLecturers()
        {
            if (!System.IO.File.Exists(_lecturersFilePath))
            {
                // Fake data
                return new List<Lecturer>
                {
                    new Lecturer { LecturerId = 1, Name = "Dr. Khumalo", Faculty = "IT", Position = "Senior Lecturer" },
                    new Lecturer { LecturerId = 2, Name = "Prof. Van Zyl", Faculty = "Engineering", Position = "Professor" },
                    new Lecturer { LecturerId = 3, Name = "Ms. Naidoo", Faculty = "Science", Position = "Junior Lecturer" },
                    new Lecturer { LecturerId = 4, Name = "Mr. Stefan", Faculty = "Eng", Position = "Junior Lecturer"}
                };
            }
            var json = System.IO.File.ReadAllText(_lecturersFilePath);
            return JsonSerializer.Deserialize<List<Lecturer>>(json) ?? new List<Lecturer>();
        }


        // ============================================================
        //ADMIN METHIDS, ADMIN METHODS, ADMIN METHods
        // GET: /Admin
        public IActionResult Index()
        {
            var claims = LoadClaims();
            var payments = LoadPayments();
            var lecturers = LoadLecturers();

            // Join data to create a list of ViewModels for the dashboard
            var reviewList = claims.Select(c =>
            {
                var payment = payments.FirstOrDefault(p => p.PayId == c.PayId);
                var lecturer = lecturers.FirstOrDefault(l => l.LecturerId == c.LecturerId);
                return new ClaimReviewViewModel
                {
                    Claim = c,
                    LecturerName = lecturer?.Name ?? "Unknown Lecturer",
                    TotalAmount = payment != null ? payment.NumHours * payment.Rate : 0
                };
            }).OrderBy(v => v.Claim.Status).ThenByDescending(v => v.Claim.ClaimTime).ToList();

            return View(reviewList);
        }

        // GET: /Admin/Review/5
        public IActionResult Review(int id)
        {
            var claims = LoadClaims();
            var payments = LoadPayments();
            var lecturers = LoadLecturers();

            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            var payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);
            var lecturer = lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);

            var viewModel = new ClaimReviewViewModel
            {
                Claim = claim,
                Payment = payment,
                LecturerName = lecturer?.Name ?? "Unknown Lecturer",
                TotalAmount = payment != null ? payment.NumHours * payment.Rate : 0,
            };

            return View(viewModel);
        }

        // POST: /Admin/ProcessReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessReview(int claimId, string action, string adminComment)
        {
            var claims = LoadClaims();
            var existingClaim = claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (existingClaim == null)
            {
                TempData["ErrorMessage"] = $"Claim ID {claimId} not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only allow processing if the claim is Pending
            if (existingClaim.Status == "Pending")
            {
                if (action == "Approve")
                {
                    existingClaim.Status = "Approved";
                    TempData["SuccessMessage"] = $"Claim {claimId} successfully Approved.";
                }
                else if (action == "Reject")
                {
                    existingClaim.Status = "Rejected";

                    existingClaim.Message = (existingClaim.Message ?? "") +
                                            $"\n(REJECTED - Admin Reason: {adminComment})";
                    TempData["SuccessMessage"] = $"Claim {claimId} successfully Rejected.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid action specified.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Claim {claimId} is already {existingClaim.Status} and cannot be modified.";
            }

            SaveClaims(claims);
            return RedirectToAction(nameof(Index));
        }
    }
}