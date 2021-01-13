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
        
        //Function to check if the reservations isn't already made before we save it in the database.
        public bool DoubleReservation(int userId, Reservation res)
        {
            foreach(var item in _context.Reservation.ToList())
            {
                if(item.PersonId == userId && item.Date == res.Date && item.Timeslot.TimeslotId == res.TimeslotId)
                {
                    return false;
                }
            }
            return true;
        }

        //Function to check if there is enough free spaces for the reservation to be made.
        //There will always be 1 place reserved for the BHV'er / ER. 
        public bool EnoughFreeSpaces(Reservation res)
        {
            int amountOfPlaces = _context.Seats.ToList()[0].places;
            int count = 0;
            bool bhv = false;
            //Count will increase by 1 for every reservation already made for the selected date and timeslot.
            foreach(var item in _context.Reservation.ToList())
            {
                if (item.Date == res.Date && item.Timeslot.TimeslotId == res.TimeslotId)
                {
                    count++;
                }
            }
            //Check wether the user is a bhv'er / er or not.
            foreach(var person in _context.Person.ToList())
            {

                foreach(var roles in _context.Role.ToList())
                {
                    if(person.PersonId == res.PersonId && person.RolesId == roles.RolesId && roles.BHV)
                    {
                        bhv = true;
                    }
                }
            }
            if (bhv)
            {
                if(count < amountOfPlaces) { return true; } 
            } else
            {
                if(count < (amountOfPlaces - 1)) { return true; } //A non-BHV'er / ER will get one place less since there is 1 place always reserved for a BHV'er / ER.
            }
            return false;
        }

        // GET: Reservation/Create
        public IActionResult Index(string personId)
        {
            checkAllReservationsForExpired();
            var AmountRes = AmountReservedPlaces();
            var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), new Popup(), personId, _context.Person.ToList(), AmountRes, Tuple.Create(_context.Seats.ToList()[0].places, _context.Teams.ToList(), _context.TeamMember.ToList()));
            return View(tuple);
        }
        // POST: Reservation

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("ReservationId,Date,Timeslot", Prefix = "Item1")] Reservation reservation, string[] selectedObjects, int[] selectedTimeslots, string personId, int[] selectedTeams)
        {
            //Check if person selected a team while reservering.
            if(selectedTeams.Length == 0) 
            {
                // The person places a reservation for him/herself alone.
                // ----------
                //New person gets created (to be added to the reservation)
                Person user = new Person();
                foreach (var item in _context.Person)
                {
                    if (item.PersonId.ToString() == personId) //For each of the persons from the database check if thats the person who reserved the spots.
                    {
                        user = item;
                    }
                }
                //Create a new timeslot array where all the correct timeslots will be stored.
                Timeslot[] rightTimeslots = new Timeslot[selectedTimeslots.Length];
                Dictionary<int, string> time = new Dictionary<int, string>();
                for (int i = 0; i < selectedTimeslots.Length; i++)
                {
                    for (int j = 0; j < timeslotList.Count; j++)
                    {
                        if (selectedTimeslots[i] == timeslotList[j].TimeslotId)
                        {
                            //Format the timeslots to the right format required by the database/model.
                            rightTimeslots[i] = timeslotList[j];
                            string s = timeslotList[j].TimeStart + "-" + timeslotList[j].TimeEnd;
                            time.Add(selectedTimeslots[i], s);
                        }
                    }
                }
                //Create a new double reservation array where all the reservations will be stored.
                //There will be 1 reservation per timeslots. So if someone selected 3 days with 2 timeslots there will be a total of 3 * 2 = 6 reservations created.
                Reservation[][] revList = new Reservation[selectedObjects.Length][];
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    revList[i] = new Reservation[selectedTimeslots.Length];
                    for (int j = 0; j < selectedTimeslots.Length; j++)
                    {
                        revList[i][j] = new Reservation();
                        revList[i][j].Date = selectedObjects[i];
                        //Add the right user to the new made reservation.
                        revList[i][j].Person = user;
                        revList[i][j].PersonId = user.PersonId;
                    }
                }
                //Add all the correctly formatted timeslots to all the reservations.
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    for (int j = 0; j < selectedTimeslots.Length; j++)
                    {
                        revList[i][j].Timeslot = rightTimeslots[j];
                        revList[i][j].TimeslotId = rightTimeslots[j].TimeslotId;
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
                            //Check for existing reservations / double reservations / enough space.
                            if (DoubleReservation(revList[j][i].PersonId, revList[j][i]) && EnoughFreeSpaces(revList[j][i]))
                            {
                                Calender(revList[j][i]);
                                popup.popupMessage += revList[j][i].Person.PersonName + "|" + revList[j][i].Date + "|" + time[revList[j][i].Timeslot.TimeslotId] + "||";
                                _context.Add(revList[j][i]);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    var AmountRes = AmountReservedPlaces();
                    var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), popup, personId, _context.Person.ToList(), AmountRes, Tuple.Create(_context.Seats.ToList()[0].places, _context.Teams.ToList(), _context.TeamMember.ToList()));
                    return View(tuple);
                    }
                    else
                    {
                        var AmountRes = AmountReservedPlaces();
                        Popup popup = new Popup();
                        popup.popupMessage = "an error has occured";
                        var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), popup, personId, _context.Person.ToList(), AmountRes, Tuple.Create(_context.Seats.ToList()[0].places, _context.Teams.ToList(), _context.TeamMember.ToList()));
                        return View(tuple);
                    }
            }
            else
            {
                Popup popup2 = new Popup();
                popup2.popupMessage = "The following reservation have been made:||";
                // The person places a reservation for him/her and a team.
                // -----------
                // Get the selected team
                foreach (var teamId in selectedTeams)
                {
                    var testvar = _context.TeamMember.ToList();
                    //Create a reservation for each member of the selected team.
                    foreach (var member in _context.TeamMember.ToList())
                    {
                        if (member.TeamId == teamId)
                        {
                            Person user = new Person();
                            foreach (var item in _context.Person)
                            {
                                if (item.PersonId.ToString() == member.PersonId.ToString()) //For each of the persons from the database check if thats the person who reserved the spots.
                                {
                                    user = item;
                                }
                            }
                            //Create a new timeslot array where all the correct timeslots will be stored.
                            Timeslot[] rightTimeslots = new Timeslot[selectedTimeslots.Length];
                            Dictionary<int, string> time = new Dictionary<int, string>();
                            for (int i = 0; i < selectedTimeslots.Length; i++)
                            {
                                for (int j = 0; j < timeslotList.Count; j++)
                                {
                                    if (selectedTimeslots[i] == timeslotList[j].TimeslotId)
                                    {
                                        //Format the timeslots to the right format required by the database/model.
                                        rightTimeslots[i] = timeslotList[j];
                                        string s = timeslotList[j].TimeStart + "-" + timeslotList[j].TimeEnd;
                                        time.Add(selectedTimeslots[i], s);
                                    }
                                }
                            }
                            //Create a new double reservation array where all the reservations will be stored.
                            //There will be 1 reservation per timeslots. So if someone selected 3 days with 2 timeslots there will be a total of 3 * 2 = 6 reservations created.
                            Reservation[][] revList = new Reservation[selectedObjects.Length][];
                            for (int i = 0; i < selectedObjects.Length; i++)
                            {
                                revList[i] = new Reservation[selectedTimeslots.Length];
                                for (int j = 0; j < selectedTimeslots.Length; j++)
                                {
                                    revList[i][j] = new Reservation();
                                    revList[i][j].Date = selectedObjects[i];
                                    //Add the right user to the new made reservation.
                                    revList[i][j].Person = user;
                                    revList[i][j].PersonId = user.PersonId;
                                }
                            }
                            //Add all the correctly formatted timeslots to all the reservations.
                            for (int i = 0; i < selectedObjects.Length; i++)
                            {
                                for (int j = 0; j < selectedTimeslots.Length; j++)
                                {
                                    revList[i][j].Timeslot = rightTimeslots[j];
                                    revList[i][j].TimeslotId = rightTimeslots[j].TimeslotId;
                                }
                            }
                            if (ModelState.IsValid && selectedObjects.Length != 0 && selectedTimeslots.Length != 0)
                            {
                                for (int j = 0; j < selectedObjects.Length; j++)
                                {
                                    for (int i = 0; i < selectedTimeslots.Length; i++)
                                    {
                                        //Check for existing reservations / double reservations / enough space.
                                        if (DoubleReservation(revList[j][i].PersonId, revList[j][i]) && EnoughFreeSpaces(revList[j][i])) {
                                            Calender(revList[j][i]);
                                            popup2.popupMessage += revList[j][i].Person.PersonName + "|" + revList[j][i].Date + "|" + time[revList[j][i].Timeslot.TimeslotId] + "||";
                                            _context.Add(revList[j][i]);
                                            await _context.SaveChangesAsync();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var AmountRes1 = AmountReservedPlaces();
                                Popup popup1 = new Popup();
                                popup1.popupMessage = "an error has occured";
                                var tuple1 = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), popup1, personId, _context.Person.ToList(), AmountRes1, Tuple.Create(_context.Seats.ToList()[0].places, _context.Teams.ToList(), _context.TeamMember.ToList()));
                                return View(tuple1);
                            }
                        }
                    }
                }
                var AmountRes = AmountReservedPlaces();
                var tuple = Tuple.Create(new Reservation(), _context.Timeslot.ToList(), popup2, personId, _context.Person.ToList(), AmountRes, Tuple.Create(_context.Seats.ToList()[0].places, _context.Teams.ToList(), _context.TeamMember.ToList()));
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

        public int[][] AmountReservedPlaces() {
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
            int[][] count = new int[10][];
            int indexI = 0;
            int indexJ = 0;
            foreach(var dt in days) {
                indexJ = 0;
                //create 2d array with amount of timeslots
                count[indexI] = new int[_context.Timeslot.ToArray().Length];
                foreach (var ts in _context.Timeslot.ToArray()) {
                    foreach (var res in _context.Reservation.ToArray())
                        //check date overlap between day & res
                        if(res.Date == dt.Date.ToShortDateString()) {
                            //check timeslot overlap between ts & res
                            if(res.Timeslot.TimeslotId == ts.TimeslotId) {
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

        //Check if there are any reservations which are 7 days or older. If there are any, delete them.
        public void checkAllReservationsForExpired()
        {
            List<int> reservationIds = new List<int>();
            DateTime now = DateTime.Now;
            foreach (var res in _context.Reservation)
            {
                DateTime dateToCheck = DateTime.Parse(res.Date);
                if(dateToCheck < now.AddDays(-7))
                {
                    reservationIds.Add(res.ReservationId);
                }
            }
            foreach(int i in reservationIds)
            {
                var reservation = _context.Reservation.Find(i);
                _context.Reservation.Remove(reservation);
                _context.SaveChanges();
            }
        }
    }
}
