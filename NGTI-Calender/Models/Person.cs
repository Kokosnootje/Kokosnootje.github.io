using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        //public ICollection<Role> Roles { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public string PersonName { get; set; }
        public string Password { get; set; }

    }
}
