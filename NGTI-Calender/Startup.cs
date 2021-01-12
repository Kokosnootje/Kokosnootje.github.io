using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using NGTI_Calender.Data;
using NGTI_Calender.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;
using System.Globalization;
using NGTI_Calender.Controllers;

namespace NGTI_Calender {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ApplicationDbContext context, IWebHostEnvironment env) {
            Timer timer = new Timer(context.Timeslot.ToList(), context.Reservation.ToList(), context.Person.ToList(), context.Role.ToList());
            Thread t1 = new Thread(timer.SetTimer);
            t1.Start();
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

        }
        
    }

    public class Timer {
        private List<Timeslot> Timeslots;
        private List<Reservation> Reservations;
        private List<Person> Persons;
        private List<Roles> Roles;
        private int lastChecked = -1;
        public Timer(List<Timeslot> ts, List<Reservation> rs, List<Person> p, List<Roles> r) {
            Timeslots = ts;
            Reservations = rs;
            Persons = p;
            Roles = r;
        }

        public void SetTimer() {
            //timer that checks when 10 minutes have passed
            var timer = new System.Timers.Timer(600000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e) {
            //check if there are no BHV in a timeslot that starts in the next 30 min (and if so cancels the reservations);
            int count = 0;
            DateTime now = DateTime.Now;
            DateTime in30Min = DateTime.Now.AddMinutes(30);
            foreach (Timeslot ts in Timeslots) {
                DateTime startTime = DateTime.Parse(ts.TimeStart);
                if ((now < startTime && startTime < in30Min) && ts.TimeslotId != lastChecked) {
                    lastChecked = ts.TimeslotId;
                    //check all reservations for the upcoming timeslot
                    foreach (var res in Reservations) {
                        if (res.Timeslot.TimeslotId == ts.TimeslotId) {
                            string[] s = res.Date.Split("-");
                            if (s[0].Length != 2) {
                                s[0] = "0" + s[0];
                            }
                            if (s[1].Length != 2) {
                                s[1] = "0" + s[1];
                            }
                            string s2 = s[0] + "/" + s[1] + "/" + s[2];
                            DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            bool a = dt > DateTime.Today;
                            bool c = dt >= DateTime.Today;
                            bool b = DateTime.Parse(res.Timeslot.TimeStart) >= DateTime.Now;
                            if (a || (b && c)) {
                                Console.WriteLine();
                                foreach (var person in Persons) {
                                    if (person.PersonId == res.PersonId) {
                                        //check if there is a BHV'er
                                        foreach (var role in Roles) {
                                            if (role.RolesId == person.RolesId) {
                                                if (role.BHV) {
                                                    count++;
                                                }
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                    //if there are no BHV'ers
                    if (count == 0) {
                        //if there are no BHV'ers
                        foreach (var res in Reservations) {
                            if (res.Timeslot.TimeslotId == ts.TimeslotId) {
                                string[] s = res.Date.Split("-");
                                if (s[0].Length != 2) {
                                    s[0] = "0" + s[0];
                                }
                                if (s[1].Length != 2) {
                                    s[1] = "0" + s[1];
                                }
                                string s2 = s[0] + "/" + s[1] + "/" + s[2];
                                DateTime dt = DateTime.ParseExact(s2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                bool a = dt > DateTime.Today;
                                bool c = dt >= DateTime.Today;
                                bool b = DateTime.Parse(res.Timeslot.TimeStart) >= DateTime.Now;
                                if (a || (b && c)) {
                                    Console.WriteLine();
                                    foreach (var person in Persons) {
                                        if (person.PersonId == res.PersonId) {
                                            //send a mail to each person that their reservation is canceled. (reservations are not removed from the database)
                                            AdminController.SendMail(res.Date, res.Timeslot.TimeStart, res.Timeslot.TimeEnd, person.EMail, "No BHV'er has made a reservation for this timeslot.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}