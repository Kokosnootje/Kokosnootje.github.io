using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        public int RolesId { get; set; }
        public Roles Roles { get; set; }
        public string PersonName { get; set; }
        public string EMail { get; set; }
        public string Image { get; set; }

        public Person() { this.Roles = new Roles(); }
    }
}
