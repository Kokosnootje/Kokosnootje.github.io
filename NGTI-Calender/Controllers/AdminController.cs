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

        public IActionResult Index(string personId, string message = "") {
            Popup popup = new Popup() { popupMessage = message };
            var tuple = Tuple.Create(_context.Timeslot.ToList(), popup, _context.Person.ToList(), personId, _context.Seats.ToList()[0], _context.Role.ToList());
            return View(tuple);
        }

        //remove timeslot from database
        [HttpPost]
        public async Task<IActionResult> Index(int timeslotId, string personId) {
            foreach (var item in _context.Timeslot.ToList()) {
                if (item.TimeslotId == timeslotId) {
                    foreach (var res in _context.Reservation.ToList()) {
                        if (res.Timeslot.TimeslotId == timeslotId) {
                            string[] s = res.Date.Split("-");
                            if (s[0].Length != 2) {
                                s[0] = "0" + s[0];
                            }
                            if (s[1].Length != 2) {
                                s[1] = "0" + s[1];
                            }
                            string s2 = s[0] + "/" + s[1] + "/" + s[2];
                            DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            bool a = dt > DateTime.Today;
                            bool c = dt >= DateTime.Today;
                            bool b = DateTime.Parse(res.Timeslot.TimeStart) >= DateTime.Now;
                            if (a || (b &&c)) {
                                Console.WriteLine();
                                foreach (var person in _context.Person.ToList()) {
                                    if (person.PersonId == res.PersonId) {
                                        SendMail(res.Date, res.Timeslot.TimeStart, res.Timeslot.TimeEnd, person.EMail);
                                    }
                                }
                            }
                            _context.Reservation.Remove(res);
                        }
                    }
                    //must cascade drop
                    _context.Timeslot.Remove(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", new { personId = personId, message = "The timeslot has been removed." });
                }
            }
            return RedirectToAction("Index", new { personId = personId, message = "An error has occured." });
        }

        //add timeslot to database
        [HttpPost]
        public async Task<IActionResult> AddTimeslot(string startTime, string endTime, string personId) {
            var timeslotList = _context.Timeslot.ToList();
            bool overlap = false;
            //check for correct input 
            try {
                if (!(string.IsNullOrEmpty(startTime) && string.IsNullOrEmpty(endTime))) {
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
                }
            } catch {
                //wrong input message
                return RedirectToAction("Index", new { personId = personId, message = "Please enter valid input" });
            }
            //check if there are null values
            if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime)) {
                return RedirectToAction("Index", new { personId = personId, message = "Please enter valid input" });
            }
            //add to database if it does not overlap
            else if (!overlap) {
                _context.Timeslot.Add(new Timeslot() { TimeStart = startTime, TimeEnd = endTime });
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { personId = personId, message = "" });
            } else {
                //overlapping input message
                return RedirectToAction("Index", new { personId = personId, message = "The input overlaps with an existing timeslot." });
            }
        }

        //change seats amount
        [HttpPost]
        public async Task<IActionResult> ChangeSeatsAmount(string amount ,string personId) {
            try {
                int places = int.Parse(amount);
                if (places >= 0) {
                    _context.Seats.ToList()[0].places = places;
                    await _context.SaveChangesAsync();
                    var tuple1 = Tuple.Create(_context.Timeslot.ToList(), new Popup(), _context.Person.ToList(), personId, _context.Seats.ToList()[0], _context.Roles.ToList());
                    return View(tuple1);
                }
            } catch (Exception) {
                return RedirectToAction("Index", new { personId = personId, message = "Please enter valid input" });
            }
            return RedirectToAction("Index", new { personId = personId, message = "Please enter valid input" });
        }

        public async Task<IActionResult> EmployeeConfig(string personId, string[] RolesIds, string[] AdminBools, string[] BHVBools) {
            for (int i = 0; i < RolesIds.Length; i++){
                foreach(var role in _context.Role.ToList()) {
                    if(role.RolesId.ToString() == RolesIds[i]) {
                        if (AdminBools[i] == "True") {
                            role.Admin = true;
                        } else {
                            role.Admin = false;
                        }
                        if (BHVBools[i] == "True") {
                            role.BHV = true;
                        } else {
                            role.BHV = false;
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { personId = personId });
        }
        public static void SendMail(string date, string timeStart, string timeEnd, string email) {
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
            mail.Body = "Your reservation for:\n" + date + "\n" + timeStart + "-" + timeEnd + "\nhas been canceled.";
            
            SmtpServer.Send(mail);
        }
    }
}

