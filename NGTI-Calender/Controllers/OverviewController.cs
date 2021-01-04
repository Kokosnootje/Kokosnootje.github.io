using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;
using System.Net.Mail;
using NGTI_Calender.Models;
using System.Globalization;

namespace NGTI_Calender.Controllers
{
    public class OverviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OverviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Overview/Index
        public IActionResult Index(string personId, string SelectedDate = "", string SelectedTimeslot = "", string AmountAvailablePlaces = "")
        {
            var amountRes = AmountReservedPlaces();
            string[] selectedReservation = new string[] { SelectedDate, SelectedTimeslot };
            var tuple = Tuple.Create(_context.Timeslot.ToList(), Tuple.Create(SelectedDate, SelectedTimeslot, personId, AmountAvailablePlaces), _context.Reservation.ToList(), _context.Person.ToList(), new Reservation(), Tuple.Create(amountRes, _context.Seats.ToList()[0].places, WhenBHV()), _context.Role.ToList());
            return View(tuple);
        }

        // POST: Overview/Index
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string resList, string personId)
        {
            string p = personId;
            string[] resids = resList.Split(' ');
            int[] reservationIds = new int[resids.Length];
            for (int j = 0; j < resids.Length; j++)
            {
                reservationIds[j] = Int32.Parse(resids[j]);
            }
            for (int i = 0; i < resids.Length; i++)
            {
                var reservation = await _context.Reservation.FindAsync(reservationIds[i]);
                _context.Reservation.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { personId = personId });
        }

        // POST: Overview/GetAllReservations
        [HttpPost, ActionName("GetAllReservations")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAllReservations(string selectedDate, string selectedTimeslot, string personId, string amountAvailablePlaces)
        {
            return RedirectToAction("Index", new { personId = personId, SelectedDate = selectedDate, SelectedTimeslot = selectedTimeslot, AmountAvailablePlaces = amountAvailablePlaces });
        }

        public async Task<IActionResult> RemoveReservation(string selectedDate, string selectedTimeslot, string personId, string amountAvailablePlaces, string reservationId) {
            foreach (var res in _context.Reservation.ToList()) {
                if (res.ReservationId.ToString() == reservationId) {
                    _context.Reservation.Remove(res);
                    await _context.SaveChangesAsync();
                    foreach(var person in _context.Person.ToList()) {
                        if (res.PersonId == person.PersonId) {
                            foreach(var ts in _context.Timeslot.ToList()) {
                                if (res.TimeslotId == ts.TimeslotId) {
                                    string[] s = res.Date.Split("-");
                                    if (s[0].Length != 2) {
                                        s[0] = "0" + s[0];
                                    }
                                    if (s[1].Length != 2) {
                                        s[1] = "0" + s[1];
                                    }
                                    string s2 = s[0] + "/" + s[1] + "/" + s[2];
                                    DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    if (DateTime.Parse(ts.TimeStart) >= DateTime.Now || dt > DateTime.Today) {
                                        AdminController.SendMail(date: res.Date, timeStart: ts.TimeStart, timeEnd: ts.TimeEnd, email: person.EMail);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
            //lower amount available places by 1
            string[] arr = amountAvailablePlaces.Split(" / ");
            try {
                int i = int.Parse(arr[0]);
                i--;
                amountAvailablePlaces = i.ToString() + " / " + arr[1];
            } catch (Exception) {
                throw;
            }
            return RedirectToAction("Index", new { personId = personId, SelectedDate = selectedDate, SelectedTimeslot = selectedTimeslot, AmountAvailablePlaces = amountAvailablePlaces });
        }


        public int[][] AmountReservedPlaces()
        {
            //load upcoming 2 weeks - weekend
            DateTime[] days = new DateTime[10];
            DateTime lastDay = DateTime.Now;
            for (int i = 0; i < 10; i++)
            {
                if (lastDay.DayOfWeek == DayOfWeek.Saturday)
                {
                    lastDay = lastDay.AddDays(2.0);
                    days[i] = lastDay;
                    lastDay = lastDay.AddDays(1.0);
                }
                else if (lastDay.DayOfWeek == DayOfWeek.Sunday)
                {
                    lastDay = lastDay.AddDays(1.0);
                    days[i] = lastDay;
                    lastDay = lastDay.AddDays(1.0);

                }
                else
                {
                    if (lastDay.DayOfWeek == DayOfWeek.Monday)
                    {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }
                    if (lastDay.DayOfWeek == DayOfWeek.Tuesday)
                    {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }
                    else if (lastDay.DayOfWeek == DayOfWeek.Wednesday)
                    {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }
                    else if (lastDay.DayOfWeek == DayOfWeek.Thursday)
                    {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }
                    else if (lastDay.DayOfWeek == DayOfWeek.Friday)
                    {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }

                }
            }
            int[][] count = new int[10][];
            int indexI = 0;
            int indexJ = 0;
            foreach (var dt in days)
            {
                indexJ = 0;
                //create 2d array with amount of timeslots
                count[indexI] = new int[_context.Timeslot.ToArray().Length];
                foreach (var ts in _context.Timeslot.ToArray())
                {
                    foreach (var res in _context.Reservation.ToArray())
                        //check date overlap between day & res
                        if (res.Date == dt.Date.ToShortDateString())
                        {
                            //check timeslot overlap between ts & res
                            if (res.Timeslot.TimeslotId == ts.TimeslotId)
                            {
                                //if overlap add count
                                count[indexI][indexJ]++;
                            }
                        }
                    indexJ++;
                }
                indexI++;
            }
            return count;
        }

        protected int[][] WhenBHV() {

            //load upcoming 2 weeks - weekend
            DateTime[] days = new DateTime[10];
            DateTime lastDay = DateTime.Now;
            for (int i = 0; i < 10; i++) {
                if (lastDay.DayOfWeek == DayOfWeek.Saturday) {
                    lastDay = lastDay.AddDays(2.0);
                    days[i] = lastDay;
                    lastDay = lastDay.AddDays(1.0);
                } else if (lastDay.DayOfWeek == DayOfWeek.Sunday) {
                    lastDay = lastDay.AddDays(1.0);
                    days[i] = lastDay;
                    lastDay = lastDay.AddDays(1.0);

                } else {
                    if (lastDay.DayOfWeek == DayOfWeek.Monday) {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }
                    if (lastDay.DayOfWeek == DayOfWeek.Tuesday) {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    } else if (lastDay.DayOfWeek == DayOfWeek.Wednesday) {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    } else if (lastDay.DayOfWeek == DayOfWeek.Thursday) {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    } else if (lastDay.DayOfWeek == DayOfWeek.Friday) {
                        days[i] = lastDay;
                        lastDay = lastDay.AddDays(1.0);
                    }

                }
            }
            int[][] arr = new int[10][];
            int indexI = 0;
            int indexJ = 0;
            foreach (var dt in days) {
                indexJ = 0;
                //create 2d array with amount of timeslots
                arr[indexI] = new int[_context.Timeslot.ToArray().Length];
                foreach (var ts in _context.Timeslot.ToArray()) {
                    foreach (var res in _context.Reservation.ToArray())
                        //check date overlap between day & res
                        if (res.Date == dt.Date.ToShortDateString()) {
                            //check timeslot overlap between ts & res
                            foreach(var person in _context.Person.ToList()) {
                                //match person + roles to the reservation
                                if(person.PersonId == res.PersonId) {
                                    foreach(var role in _context.Role.ToList()) {
                                        if (person.RolesId == role.RolesId) {
                                            if (role.BHV && res.Timeslot.TimeslotId == ts.TimeslotId) {
                                                //if person is a BHV add count
                                                arr[indexI][indexJ]++;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    indexJ++;
                }
                indexI++;
            }
            return arr;
        }
    }
}
