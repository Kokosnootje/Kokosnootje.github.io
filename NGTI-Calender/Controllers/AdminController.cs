using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;

namespace NGTI_Calender.Controllers {
    public class AdminController : Controller {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context) {
            _context = context;
        }

        public IActionResult Index() {
            return View(_context.Timeslot.ToArray());
        }
    }
}
