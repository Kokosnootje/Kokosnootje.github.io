using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models
{
    public class Timeslot
    {
        public int TimeslotId { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
    }
}
