using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NGTI_Calender.Models;

namespace NGTI_Calender.Data
{
    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }
        public DbSet<NGTI_Calender.Models.Person> Person { get; set; }
        public DbSet<NGTI_Calender.Models.Reservation> Reservation { get; set; }
        public DbSet<NGTI_Calender.Models.Timeslot> Timeslot { get; set; }
        public DbSet<NGTI_Calender.Models.Roles> Role { get; set; }
        public DbSet<NGTI_Calender.Models.Seats> Seats { get; set; }
    }
}
