using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;

namespace NGTI_Calender.Controllers {
    public class OverviewController : Controller {
        private readonly ApplicationDbContext _context;

        public OverviewController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Overview/Index
        public IActionResult Index(string personId) {
            var tuple = Tuple.Create(_context.Timeslot.ToList(), personId, _context.Reservation.ToList(), _context.Person.ToList());
            return View(tuple);
        }
    }
}
