using System;
using System.Collections.Generic;
using System.Globalization;
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
        public IActionResult Index(string personId, string[] reservations = null, string[] timeslots = null, List<string> personList = null) {
            var tuple = Tuple.Create(personId, _context.Person.ToList(), _context.Teams.ToList(), _context.TeamMember.ToList(), reservations, timeslots, personList);
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

        public async Task<IActionResult> show_team(string PersonId, string TeamId)
        {
            int i = 0;
            List<string> PersonStringList = new List<string>();
            List<Person> PersonList = new List<Person>();
            foreach (TeamMember tm in _context.TeamMember.ToList())
            {
                if (tm.TeamId.ToString() == TeamId)
                {
                    i++;
                    foreach (Person p in _context.Person.ToList())
                    {
                        if (p.PersonId == tm.PersonId)
                        {
                            PersonStringList.Add(p.PersonName);
                            PersonList.Add(p);
                        }
                    }

                }
            }

            int e = 0;
            Reservation[][] resList = new Reservation[i][];
            foreach (Person p2 in PersonList) {
                resList[e] = new Reservation[GetAmount(p2)];
                if (e < i) {
                    int w = 0;
                    foreach (Reservation res in _context.Reservation.ToList()) {
                        string[] s = res.Date.Split("-");
                        if (s[0].Length != 2) {
                            s[0] = "0" + s[0];
                        }
                        if (s[1].Length != 2) {
                            s[1] = "0" + s[1];
                        }
                        string s2 = s[0] + "/" + s[1] + "/" + s[2];
                        DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        if (dt >= DateTime.Today && res.PersonId == p2.PersonId) {
                            resList[e][w] = (res);
                            w++;
                        }
                    }
                }
                e++;
            }
            string[] reservations = new string[i];
            string[] timeslots = new string[i];
            int c = 0;
            int o = 0;
            foreach (var reslist in resList) {
                List<Reservation> sortedResList = OverviewController.sortAllRes(reslist.ToList());
                string s3 = "";
                string s4 = "";
                foreach (var res2 in sortedResList) {
                    foreach (var ts in _context.Timeslot.ToList()) {
                        if (ts.TimeslotId == res2.TimeslotId) {
                            s4 += ts.TimeStart + "-" + ts.TimeEnd + "|";
                        }
                    }
                    s3 += res2.Date + "|";
                    o++;
                }
                reservations[c] = s3;
                timeslots[c] = s4;
                c++;
            }
            //string[] reservations = new string[i];
            //string[] timeslots = new string[i];
            //int c = 0;
            //foreach (TeamMember tm2 in _context.TeamMember.ToList())
            //{
            //    if( c < i)
            //        {


            //        int p = 0;
            //        string s3 = "";
            //        string s4 = "";
            //        foreach (Reservation res in _context.Reservation.ToList())
            //        {
            //            string[] s = res.Date.Split("-");
            //            if (s[0].Length != 2)
            //            {
            //                s[0] = "0" + s[0];
            //            }
            //            if (s[1].Length != 2)
            //            {
            //                s[1] = "0" + s[1];
            //            }
            //            string s2 = s[0] + "/" + s[1] + "/" + s[2];
            //            DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //            if  (dt >= DateTime.Today && res.PersonId == tm2.PersonId)
            //            {
            //                foreach(var ts in _context.Timeslot.ToList()) {
            //                    if(ts.TimeslotId == res.TimeslotId) {
            //                        s4 += ts.TimeStart + "-" + ts.TimeEnd + "|";
            //                    }
            //                }
            //                s3 += res.Date + "|";
            //                p++;
            //            }
            //        }
            //        reservations[c] = s3;
            //        timeslots[c] = s4;
            //        }
            //    c++;
            //}
            return RedirectToAction("Index", new { personId = PersonId, reservations = reservations, timeslots = timeslots, personList = PersonStringList });
        }

        public int GetAmount(Person p)
        {
            int t = 0;
            foreach (Reservation res in _context.Reservation.ToList())
            {
                string[] s = res.Date.Split("-");
                if (s[0].Length != 2)
                {
                    s[0] = "0" + s[0];
                }
                if (s[1].Length != 2)
                {
                    s[1] = "0" + s[1];
                }
                string s2 = s[0] + "/" + s[1] + "/" + s[2];
                DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (dt >= DateTime.Today && res.PersonId == p.PersonId)
                {
                    t++;
                }
            }
            return t;
                
        }

        
        // POST: Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,TeamName")] Team team, int[] selectedPersons, string PersonId) {
            if (ModelState.IsValid) {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();
                foreach (var id in selectedPersons) {
                    _context.TeamMember.Add(new TeamMember() { TeamId = team.TeamId, PersonId = id });
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index", new {PersonId });
            }
            return RedirectToAction("Index", new {PersonId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeamMember(string teamId, string personId, string rmpersonId) {
            foreach (Team team in _context.Teams.ToList()) {
                foreach (TeamMember tm in _context.TeamMember.ToList()) {
                    if (tm.TeamId.ToString() == teamId) {
                        foreach (Person person in _context.Person.ToList()) {
                            if (tm.PersonId.ToString() == rmpersonId) {
                                _context.TeamMember.Remove(tm);
                                await _context.SaveChangesAsync();
                                return RedirectToAction("Index", new { personId = personId});
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { personId = personId });

        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeam(string teamId, string personId) {
            foreach (Team team in _context.Teams.ToList()) {
                if (teamId == team.TeamId.ToString()) {
                    _context.Teams.Remove(team);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", new { personId = personId });

                }
            }
            return RedirectToAction("Index", new { personId = personId });
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
