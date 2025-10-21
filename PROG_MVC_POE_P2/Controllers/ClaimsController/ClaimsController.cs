using Microsoft.AspNetCore.Mvc;
using PROG_MVC_POE_P2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace PROG_MVC_POE_P2.Controllers
{
    public class ClaimController : Controller
    {
        // Simulated in-memory storage
        private static List<Claim> claims = new List<Claim>();
        private static List<Payment> payments = new List<Payment>();
        private static List<Lecturer> lecturers = new List<Lecturer>();

        private readonly IWebHostEnvironment _env;

        public ClaimController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // GET: Claim
        public IActionResult Index()
        {      
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
                // Create the new claim
                var claim = new Claim(); 
                claim.LecturerId = LecturerId; 

                // Handle file upload
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

                // Set system-generated fields
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

                // Add claim to list
                claims.Add(claim);

                return RedirectToAction(nameof(Index));
            }

            // TempData["ErrorMessage"] = "All fields are required.";
            return View(); 
        }

        // GET: Claim/Edit/5
        public IActionResult Edit(int id)
        {
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

            return RedirectToAction(nameof(Index));
        }

        // GET: Claim/Details/5
        public IActionResult Details(int id)
        {
            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            var payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);
            ViewBag.Payment = payment;

            return View(claim);
        }

        // GET: Claim/Delete/5
        public IActionResult Delete(int id)
        {
            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            return View(claim);
        }

        // POST: Claim/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim != null)
            {
                var payment = payments.FirstOrDefault(p => p.PayId == claim.PayId);
                if (payment != null)
                    payments.Remove(payment);

                claims.Remove(claim);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
