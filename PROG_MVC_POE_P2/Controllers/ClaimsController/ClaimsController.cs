using Microsoft.AspNetCore.Mvc;
using PROG_MVC_POE_P2.Models;

namespace PROG_MVC_POE_P2.Controllers.Request
{
    public class ClaimsController : Controller
    {
        private static List<LecturerModel> _lecturer = new List<LecturerModel>
        {
            new LecturerModel { LecturerId = 1, Fname = "John", Lname = "Doe", NumHours = 48, Rate = 200 },
            new LecturerModel { LecturerId = 2, Fname = "Peter", Lname = "Doe", NumHours = 50, Rate = 180 }
        };

        private static List<Claim> _claims = new List<Claim>();

        public IActionResult Index()
        {
            ViewBag.Lecturers = _lecturer;
            return View(_claims);
        }


        public IActionResult Create()
        {
            ViewBag.Lecturers = _lecturer;
            return View("SubmitClaim");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitClaim(Claim claim)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Lecturers = _lecturer;
                return View("SubmitClaim", claim);
            }

            claim.ClaimId = _claims.Any() ? _claims.Max(r => r.ClaimId) + 1 : 1;
            claim.Status = "Pending";
            _claims.Add(claim);

            return RedirectToAction(nameof(Index));
        }


    }
}
