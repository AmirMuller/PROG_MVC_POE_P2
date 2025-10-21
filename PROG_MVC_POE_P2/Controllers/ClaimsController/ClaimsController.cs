using Microsoft.AspNetCore.Mvc;
using PROG_MVC_POE_P2.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace PROG_MVC_POE_P2.Controllers
{
    public class ClaimController : Controller
    {
        // Mock lecturer data
        private static List<Lecurer> _lecturers = new List<Lecurer>
        {
            new Lecurer { LecturerId = 1, Name = "Dr. John Doe", Faculty = "Humanities", Position = "Senior Lecturer" },
            new Lecurer { LecturerId = 2, Name = "Prof. Jane Smith", Faculty = "Information Technology", Position = "Professor" },
            new Lecurer { LecturerId = 3, Name = "Mr. Sam Brown", Faculty = "Commerce", Position = "Lecturer" }
        };

        // In-memory claims
        private static List<Claim> _claims = new List<Claim>();

        // GET: Claim/Index
        public IActionResult Index()
        {
            return View(_claims);
        }

        // GET: Claim/Create
        public IActionResult Create()
        {
            ViewBag.Lecturers = _lecturers;
            return View();
        }

        // POST: Claim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Claim claim)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Lecturers = _lecturers;
                return View(claim);
            }

            claim.ClaimId = _claims.Any() ? _claims.Max(c => c.ClaimId) + 1 : 1;
            claim.Status = "Pending";
            claim.ClaimTime = claim.ClaimTime == default ? DateTime.Now : claim.ClaimTime;
            _claims.Add(claim);

            return RedirectToAction(nameof(Index));
        }

        // GET: Claim/Details/5
        public IActionResult Details(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();
            return View(claim);
        }

        // GET: Claim/Edit/5
        public IActionResult Edit(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();
            ViewBag.Lecturers = _lecturers;
            return View(claim);
        }

        // POST: Claim/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Claim updated)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Lecturers = _lecturers;
                return View(updated);
            }

            claim.LecturerId = updated.LecturerId;
            claim.ClaimTime = updated.ClaimTime;
            claim.Status = updated.Status;

            return RedirectToAction(nameof(Index));
        }

        // GET: Claim/Delete/5
        public IActionResult Delete(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();
            return View(claim);
        }

        // POST: Claim/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim != null) _claims.Remove(claim);
            return RedirectToAction(nameof(Index));
        }

        // Optional Approve/Reject
        public IActionResult Approve(int id, string status)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();
            claim.Status = status;
            return RedirectToAction(nameof(Index));
        }
    }
}
