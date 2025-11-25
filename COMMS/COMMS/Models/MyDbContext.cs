using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace COMMS.Models
{
    public class MyDbContext:DbContext
    {
        public MyDbContext() :base( "MyDbContext"){ }

        public DbSet<Users> Users { get; set; }
        public DbSet<Appointment> Appointments  { get; set; }
        public DbSet<Schedule> Schedules   { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
    }
}