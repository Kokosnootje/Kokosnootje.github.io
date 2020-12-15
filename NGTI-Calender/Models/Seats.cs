using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NGTI_Calender.Models {
    public class Seats {
        [Key]
        public int SeatsId { get; set; }
        public int places { get; set; }
    }
}
