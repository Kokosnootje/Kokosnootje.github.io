using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;
using NGTI_Calender.Models;

namespace NGTI_Calender.Controllers {
    public class AdminController : Controller {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context) {
            _context = context;
        }

        public IActionResult Index(string personId) {
            var tuple = Tuple.Create(_context.Timeslot.ToList(), new Popup(), _context.Person.ToList(), personId);
            return View(tuple);
        }

        //remove timeslot from database
        [HttpPost]
        public async Task<IActionResult> Index(int timeslotId) {
            foreach (var item in _context.Timeslot.ToList()) {
                if (item.TimeslotId == timeslotId) {
                    foreach (var res in _context.Reservation.ToList()) {
                        if (res.Timeslot.TimeslotId == timeslotId) {
                            string[] s = res.Date.Split("-");
                            if (s[0].Length != 2) {
                                s[0] = "0" + s[0];
                            }
                            string s2 = s[0] + "/" + s[1] + "/" + s[2];
                            DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            bool a = dt > DateTime.Today;
                            bool c = dt >= DateTime.Today;
                            bool b = DateTime.Parse(res.Timeslot.TimeStart) >= DateTime.Now;
                            if (a || (b &&c)) {
                                //SendMail(res.Date, res.Timeslot.TimeStart, res.Timeslot.TimeEnd, res.PersonId);
                            }
                            _context.Reservation.Remove(res);
                        }
                    }
                    //must cascade drop
                    _context.Timeslot.Remove(item);
                    await _context.SaveChangesAsync();
                    var tuple2 = Tuple.Create(_context.Timeslot.ToList(), new Popup() { popupMessage = "The timeslot has been removed." }, _context.Person.ToList());
                    return View(tuple2);
                }
            }
            var tuple = Tuple.Create(_context.Timeslot.ToList(), new Popup() { popupMessage="An error has occured." }, _context.Person.ToList());
            return View(tuple);
        }

        //add timeslot to database
        [HttpPost]
        public async Task<IActionResult> AddTimeslot(string startTime, string endTime) {
            var timeslotList = _context.Timeslot.ToList();
            bool overlap = false;
            //check for correct input 
            try {
                string[] s = startTime.Split(":");
                if (s[0].Length == 1) {
                    s[0] = "0" + s[0];
                }
                startTime = s[0] + ":" + s[1];
                s = endTime.Split(":");
                if (s[0].Length == 1) {
                    s[0] = "0" + s[0];
                }
                endTime = s[0] + ":" + s[1];
                var startA = DateTime.Parse(startTime);
                var endA = DateTime.Parse(endTime);
                //check if time overlaps
                foreach (var item in timeslotList) {
                    var startB = DateTime.Parse(item.TimeStart);
                    var endB = DateTime.Parse(item.TimeEnd);
                    overlap = !(startA >= endB || startB >= endA) || overlap;
                }
            } catch {
                //wrong input message
                var tuple1 = Tuple.Create(_context.Timeslot.ToList(), new Popup { popupMessage = "Please enter valid input." }, _context.Person.ToList());
                return View(tuple1);
            }
            //check if there are null values
            if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime)){
                var tuple = Tuple.Create(_context.Timeslot.ToList(), new Popup { popupMessage = "Please enter valid input." }, _context.Person.ToList());
                return View(tuple);
            }
            //add to database if it does not overlap
            else if (!overlap) {
                _context.Timeslot.Add(new Timeslot() { TimeStart = startTime, TimeEnd = endTime });
                await _context.SaveChangesAsync();
                //timeslot has been added message
                var tuple2 = Tuple.Create(_context.Timeslot.ToList(), new Popup { popupMessage = "The timeslot has been added." }, _context.Person.ToList());
                return View(tuple2);
            } else {
                //overlapping input message
                var tuple = Tuple.Create(_context.Timeslot.ToList(), new Popup { popupMessage = "The input overlaps with an existing timeslot." }, _context.Person.ToList());
                return View(tuple);
            }
        }

        private void SendMail(string date, string timeStart, string timeEnd, int personId) {
            string email = "";
            foreach (var person in _context.Person.ToList()) {
                if (person.PersonId == personId) {
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
            mail.Subject = "Your reservation has been canceled.";
            mail.Body = "Your reservation for " + date + " | " + timeStart + "-" + timeEnd + "has been canceled";
            
            SmtpServer.Send(mail);
        }
    }
}

