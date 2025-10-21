using Microsoft.AspNetCore.Mvc;
using PROG_MVC_POE_P2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PROG_MVC_POE_P2.Controllers
{
    public class ClaimController : Controller
    {
        // Removed the static lists

        private readonly string _claimsFilePath;
        private readonly string _paymentsFilePath;
        private readonly string _lecturersFilePath;

        private readonly IWebHostEnvironment _env;

        public ClaimController(IWebHostEnvironment env)
        {
            _env = env;
            var dataPath = Path.Combine(_env.ContentRootPath, "Data");

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            //set the full file paths
            _claimsFilePath = Path.Combine(dataPath, "claims.json");
            _paymentsFilePath = Path.Combine(dataPath, "payments.json");
        }

        //=============================================================================
        //HELPER methods
        private List<Claim> LoadClaims()
        {
            if (!System.IO.File.Exists(_claimsFilePath))
            {
                return new List<Claim>();
            }

            var json = System.IO.File.ReadAllText(_claimsFilePath);
            // Deserialize text to list
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
            if (!System.IO.File.Exists(_paymentsFilePath))
            {
                return new List<Payment>();
            }
            var json = System.IO.File.ReadAllText(_paymentsFilePath);
            return JsonSerializer.Deserialize<List<Payment>>(json) ?? new List<Payment>();
        }

        private void SavePayments(List<Payment> payments)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(payments, options);
            System.IO.File.WriteAllText(_paymentsFilePath, json);
        }

        // =========================================================================================


        // GET: Claim
        public IActionResult Index()
        {
            //LOAD DATA
            var claims = LoadClaims();
            var payments = LoadPayments();

            var claimViewList = claims.Select(c =>
            {
                var payment = payments.FirstOrDefault(p => p.PayId == c.PayId);
                return new
                {
                    c.ClaimId,
                    c.LecturerId,
                    c.ClaimTime,
                    c.Status,
                    Payment = payment,
                    Total = payment != null ? payment.NumHours * payment.Rate : 0
                };
            }).ToList();

            ViewBag.ClaimPayments = claimViewList;

            return View(claims);
        }

        // GET: Claim/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Claim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int LecturerId, IFormFile uploadedFile, double rate, int numHours)
        {

            if (LecturerId > 0 && rate > 0 && numHours > 0)
            {
                //LOAD DATA
                var claims = LoadClaims();
                var payments = LoadPayments();

                //new claim
                var claim = new Claim();
                claim.LecturerId = LecturerId;

                //Handle file uploads
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }

                    claim.FilePath = "/uploads/" + uniqueFileName;
                }

                claim.ClaimId = claims.Count > 0 ? claims.Max(c => c.ClaimId) + 1 : 1;
                claim.ClaimTime = DateTime.Now;
                claim.Status = "Pending";

                // Create and link Payment
                var payment = new Payment
                {
                    PayId = payments.Count > 0 ? payments.Max(p => p.PayId) + 1 : 1,
                    NumHours = numHours,
                    Rate = rate
                };

                payments.Add(payment);
                claim.PayId = payment.PayId;

                claims.Add(claim);

                //SAVE DATA
                SavePayments(payments);
                SaveClaims(claims);

                return RedirectToAction(nameof(Index));
            }

            // TempData["ErrorMessage"] = "All fields are required.";
            return View();
        }

        // GET: Claim/Edit/5
        public IActionResult Edit(int id)
        {
            //LOAD DATA
            var claims = LoadClaims();
            var payments = LoadPayments();

            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            var payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);
            ViewBag.Payment = payment;
            return View(claim);
        }

        // POST: Claim/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Claim claim, Payment payment) 
        {
            //LOAD DATA
            var claims = LoadClaims();
            var payments = LoadPayments();

            var existingClaim = claims.FirstOrDefault(c => c.ClaimId == claim.ClaimId);
            if (existingClaim == null) return NotFound();

            var existingPayment = payments.FirstOrDefault(p => p.PayId == existingClaim.PayId);
            if (existingPayment != null)
            {
                existingPayment.NumHours = payment.NumHours;
                existingPayment.Rate = payment.Rate;
            }

            existingClaim.LecturerId = claim.LecturerId;
            existingClaim.ClaimTime = DateTime.Now; 
            existingClaim.Status = "Pending";       

            //SAVE DATA
            SavePayments(payments);
            SaveClaims(claims);

            return RedirectToAction(nameof(Index));
        }

        // GET: Claim/Details/5
        public IActionResult Details(int id)
        {
            //LOAD DATA
            var claims = LoadClaims();
            var payments = LoadPayments();

            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            var payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);
            ViewBag.Payment = payment;

            return View(claim);
        }

        // GET: Claim/Delete/5
        public IActionResult Delete(int id)
        {
            //LOAD DATA
            var claims = LoadClaims();

            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            // Load payment info for the delete
            var payments = LoadPayments();
            ViewBag.Payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);

            return View(claim);
        }

        // POST: Claim/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            //LOAD DATA
            var claims = LoadClaims();
            var payments = LoadPayments();

            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim != null)
            {
                var payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);
                if (payment != null)
                    payments.Remove(payment);

                claims.Remove(claim);

                //SAVE DATA
                SavePayments(payments);
                SaveClaims(claims);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}