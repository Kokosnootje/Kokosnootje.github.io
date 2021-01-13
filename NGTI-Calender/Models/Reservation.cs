﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        // public Office office {get; set;}
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public int TimeslotId { get; set; }
        public Timeslot Timeslot { get; set; }
        public string Date { get; set; }
    }
}
