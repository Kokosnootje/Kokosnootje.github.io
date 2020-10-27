using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NGTI_Calender.Data;
using NGTI_Calender.Models;

namespace NGTI_Calender.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservation/Create
        public IActionResult Index()
        {
            Tuple<Reservation, IEnumerable<Timeslot>> tuple = Tuple.Create<Reservation, IEnumerable<Timeslot>>(new Reservation(), _context.Timeslot.ToList());
            return View(tuple);
        }
        // POST: Reservation

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("ReservationId,Date", Prefix = "Item1")] Reservation reservation, string[] selectedObjects)
        {
            Reservation[] revList = new Reservation[selectedObjects.Length];
            for(int i = 0; i < selectedObjects.Length; i++)
            {
                revList[i] = new Reservation();
                revList[i].Date = selectedObjects[i];
            }
            if (ModelState.IsValid)
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {

                    _context.Add(revList[i]);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(await _context.Reservation.ToListAsync());
            }
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reservation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationId,Date")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reservation);
        }

        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        // POST: Reservation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,Date")] Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.ReservationId))
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
            return View(reservation);
        }

        // GET: Reservation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservation.FindAsync(id);
            _context.Reservation.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservation.Any(e => e.ReservationId == id);
        }
    }
}
