using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;
using NGTI_Calender.Models;

namespace NGTI_Calender.Controllers {
    public class OverviewController : Controller {
        private readonly ApplicationDbContext _context;

        public OverviewController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Overview/Index
        public IActionResult Index(string personId) {
            var tuple = Tuple.Create(_context.Timeslot.ToList(), personId, _context.Reservation.ToList(), _context.Person.ToList(), new Reservation());
            return View(tuple);
        }

        // POST: Overview/Index
        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string resList, string personId)
        {
            string p = personId;
            string[] resids = resList.Split(' ');
            int[] reservationIds = new int[resids.Length];
            for(int j = 0; j < resids.Length; j++)
            {
                reservationIds[j] = Int32.Parse(resids[j]);
            }
            for(int i = 0; i < resids.Length; i++)
            {
               var reservation = await _context.Reservation.FindAsync(reservationIds[i]);
               _context.Reservation.Remove(reservation);
               await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { personId = personId});
        }
    }
}
