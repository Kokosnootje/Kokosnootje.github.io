using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NGTI_Calender.Data;
using System.Net.Mail;

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

        // SEND MAIL + RETURN VIEW
        [HttpPost]
        public async Task<IActionResult> Index(int personId, string subject, string body) {
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
            mail.Subject = subject;
            mail.Body = body;

            //Json bestand met films openen en lezenmail.Body = "Beste klant. Uw reservering is ontvangen en verwerkt. Laat deze mail zien in de bioscoop als toegangsbewijs. Geniet van de film!";
            SmtpServer.Send(mail);
            return View(_context.Timeslot.ToArray());
        }
    }
}
