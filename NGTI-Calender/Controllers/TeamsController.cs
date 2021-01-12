using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGTI_Calender.Data;
using NGTI_Calender.Models;

namespace NGTI_Calender.Controllers {
    public class TeamsController : Controller {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Team
        public IActionResult Index(string personId) {
            var tuple = Tuple.Create(personId, _context.Person.ToList());
            return View(tuple);
        }

        // GET: Team/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null) {
                return NotFound();
            }

            return View(team);
        }

        // GET: Team/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Team/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,TeamName")] Team team, int[] selectedPersons) {
            if (ModelState.IsValid) {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();
                foreach (var id in selectedPersons) {
                    _context.TeamMember.Add(new TeamMember() { TeamId = team.TeamId, PersonId = id });
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        public void AddMembers(int teamId, int personId) {
            _context.TeamMember.Add(new TeamMember() { TeamId = teamId, PersonId = personId });
            _context.SaveChangesAsync();
        }

        // GET: Team/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null) {
                return NotFound();
            }
            return View(team);
        }

        // POST: Team/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,TeamName")] Team team) {
            if (id != team.TeamId) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!TeamExists(team.TeamId)) {
                        return NotFound();
                    } else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Team/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null) {
                return NotFound();
            }

            return View(team);
        }

        // POST: Team/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var team = await _context.Teams.FindAsync(id);
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id) {
            return _context.Teams.Any(e => e.TeamId == id);
        }
    }
}
