﻿using System;
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

        // GET: Reservation/Create
        public IActionResult Index() {
            return View(_context.Timeslot.ToArray());
        }
    }
}