using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        // public Office office {get; set;}
        public Person PersonId { get; set; }
        public DateTime Date { get; set; }
        public string TestDateString { get; set; }
    }
}
