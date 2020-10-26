using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGTI_Calender.Data;
using NGTI_Calender.Models;

namespace NGTI_Calender.Controllers
{
    public class TimeslotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimeslotController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Timeslot
        public async Task<IActionResult> Index()
        {
            return View(await _context.Timeslot.ToListAsync());
        }

        // GET: Timeslot/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeslot = await _context.Timeslot
                .FirstOrDefaultAsync(m => m.TimeslotId == id);
            if (timeslot == null)
            {
                return NotFound();
            }

            return View(timeslot);
        }

        // GET: Timeslot/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Timeslot/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TimeslotId,TimeStart,TimeEnd")] Timeslot timeslot)
        {
            if (ModelState.IsValid)
            {
                _context.Add(timeslot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(timeslot);
        }

        // GET: Timeslot/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeslot = await _context.Timeslot.FindAsync(id);
            if (timeslot == null)
            {
                return NotFound();
            }
            return View(timeslot);
        }

        // POST: Timeslot/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TimeslotId,TimeStart,TimeEnd")] Timeslot timeslot)
        {
            if (id != timeslot.TimeslotId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(timeslot);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TimeslotExists(timeslot.TimeslotId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(timeslot);
        }

        // GET: Timeslot/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeslot = await _context.Timeslot
                .FirstOrDefaultAsync(m => m.TimeslotId == id);
            if (timeslot == null)
            {
                return NotFound();
            }

            return View(timeslot);
        }

        // POST: Timeslot/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var timeslot = await _context.Timeslot.FindAsync(id);
            _context.Timeslot.Remove(timeslot);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TimeslotExists(int id)
        {
            return _context.Timeslot.Any(e => e.TimeslotId == id);
        }
    }
}
