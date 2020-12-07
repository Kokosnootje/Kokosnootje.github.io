using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models {
    public class Roles {
        [Key]
        public int RolesId { get; set; }
        public bool BHV { get; set; }
        public bool Admin { get; set; }

        public Roles() { this.BHV = false; this.Admin = false; }
    }
}
