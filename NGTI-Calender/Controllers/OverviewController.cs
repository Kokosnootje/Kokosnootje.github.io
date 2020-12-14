using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;
using System.Net.Mail;
using NGTI_Calender.Models;

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
        public IActionResult Index(string personId)
        {
            var amountRes = AmountReservedPlaces();
            var tuple = Tuple.Create(_context.Timeslot.ToList(), personId, _context.Reservation.ToList(), _context.Person.ToList(), new Reservation(), amountRes, _context.Seats.ToList()[0].places);
            return View(tuple);
        }

        // SEND MAIL + RETURN VIEW
        [HttpPost]
        public async Task<IActionResult> Index(int personId, string subject, string body)
        {
            string email = "";
            foreach (var person in _context.Person.ToList())
            {
                if (person.PersonId == personId)
                {
                    email = person.EMail;
                }
            }
            // Server settings
            SmtpClient SmtpServer = new SmtpClient();
            SmtpServer.Port = 587;
            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.EnableSsl = true;
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("mailcinemaconfirmation@gmail.com", "ProjectB");

            // Mail reciever and the body of the mail
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("mailcinemaconfirmation@gmail.com");
            mail.To.Add(new MailAddress(email));
            mail.Subject = subject;
            mail.Body = body;

            //Json bestand met films openen en lezenmail.Body = "Beste klant. Uw reservering is ontvangen en verwerkt. Laat deze mail zien in de bioscoop als toegangsbewijs. Geniet van de film!";
            SmtpServer.Send(mail);
            return View(_context.Timeslot.ToArray());
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
    }
}
