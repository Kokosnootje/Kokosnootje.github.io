using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization.Internal;
using NGTI_Calender.Data;
using NGTI_Calender.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Text;
using System.Threading;

namespace NGTI_Calender.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public List<Timeslot> timeslotList;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
            timeslotList = _context.Timeslot.ToList();
        }

        // GET: Reservation/Create
        public IActionResult Index(string personId)
        {
            var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), new Popup(), personId, _context.Person.ToList());
            return View(tuple);
        }
        // POST: Reservation

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("ReservationId,Date,Timeslot", Prefix = "Item1")] Reservation reservation, string[] selectedObjects, int[] selectedTimeslots, string personId)
        {
            Person user = new Person();
            foreach(var item in _context.Person)
            {
                if(item.PersonId.ToString() == personId)
                {
                    user = item;
                }
            }
            Timeslot[] rightTimeslots = new Timeslot[selectedTimeslots.Length];
            Dictionary<int, string> time = new Dictionary<int, string>();
            for(int i = 0; i < selectedTimeslots.Length; i++)
            {
                for(int j = 0; j < timeslotList.Count; j++)
                {
                    if(selectedTimeslots[i] == timeslotList[j].TimeslotId)
                    {
                        rightTimeslots[i] = timeslotList[j];
                        string s = timeslotList[j].TimeStart + "-" + timeslotList[j].TimeEnd;
                        time.Add(selectedTimeslots[i], s);
                    }
                }
            }
            Reservation[][] revList = new Reservation[selectedObjects.Length][];

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                revList[i] = new Reservation[selectedTimeslots.Length];
                for(int j = 0; j < selectedTimeslots.Length; j++)
                {
                    revList[i][j] = new Reservation();
                    revList[i][j].Date = selectedObjects[i];
                    revList[i][j].Person = user;
                }
            }
            for(int i = 0; i < selectedObjects.Length; i++)
            {
                for(int j = 0; j < selectedTimeslots.Length; j++)
                {
                    revList[i][j].Timeslot = rightTimeslots[j];
                }
            }
            if (ModelState.IsValid && selectedObjects.Length != 0 && selectedTimeslots.Length != 0)
            {
                Popup popup = new Popup();
                popup.popupMessage = "The following reservation have been made:||";
                for (int j = 0; j < selectedObjects.Length; j++)
                {
                    for (int i = 0; i < selectedTimeslots.Length; i++)
                    {
                        Calender(revList[j][i]);
                        popup.popupMessage += revList[j][i].Person.PersonName + "|" + revList[j][i].Date + "|" + time[revList[j][i].Timeslot.TimeslotId] + "||";
                        _context.Add(revList[j][i]);
                        await _context.SaveChangesAsync();
                    }
                }
                var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), popup, personId, _context.Person.ToList());
                return View(tuple);
            }
            else
            {
                Popup popup = new Popup();
                popup.popupMessage = "an error has occured";
                var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), popup, personId, _context.Person.ToList());
                return View(tuple);
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

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        static string[] Scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "NGTI-Calender";
        public void Calender(Reservation newReservation)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Refer to the .NET quickstart on how to setup the environment:
            // https://developers.google.com/calendar/quickstart/dotnet
            // Change the scope to CalendarService.Scope.Calendar and delete any stored
            // credentials.
            string startTime = DateTime.Parse(newReservation.Date + " " + newReservation.Timeslot.TimeStart).ToString("yyyy-MM-ddTHH:mm:ss");
            string endTime = DateTime.Parse(newReservation.Date + " " + newReservation.Timeslot.TimeEnd).ToString("yyyy-MM-ddTHH:mm:ss");
            string personMail = newReservation.Person.EMail;
            Event newEvent = new Event()
            {
                Summary = "Going to work",
                Location = "30K Delftseplein, Rotterdam, 3013 AA",
                Description = "Planned to be at the office",
                Start = new EventDateTime()
                {
                    DateTime = DateTime.Parse(startTime),
                    TimeZone = "Europe/Amsterdam",
                },
                End = new EventDateTime()
                {
                    DateTime = DateTime.Parse(endTime),
                    TimeZone = "Europe/Amsterdam",
                },
                Attendees = new EventAttendee[] {
                    new EventAttendee() { Email = personMail }
                }
            };

            // String calendarId = "primary"; // Standaard Calender van degene die ingelogged is
            String calendarId = "s1ho1vvabdm0s2oabtmldef7e8@group.calendar.google.com";
            EventsResource.InsertRequest request = service.Events.Insert(newEvent, calendarId);
            Event createdEvent = request.Execute();
            Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
        }
    }
}
