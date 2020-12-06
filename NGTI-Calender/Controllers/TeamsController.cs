using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NGTI_Calender.Data;

namespace NGTI_Calender.Controllers {

    public class TeamsController : Controller {
        private readonly ApplicationDbContext _context;
        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(string personId) {
            var tuple = Tuple.Create(personId, _context.Person.ToList());
            return View(tuple);
        }
    }
}
