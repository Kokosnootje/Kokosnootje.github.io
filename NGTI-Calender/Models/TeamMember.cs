using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models {
    public class TeamMember {
        [Key, Column(Order=1)]
        public int TeamId { get; set; }
        public Team Team { get; set; }
        [Key, Column(Order =1)]
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }
}
