using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static log4net.Appender.RollingFileAppender;

namespace COMMS.Models
{
    public class Schedule
    {

        public int ScheduleID { get; set  ;}
        public int DoctorID { get; set; }
        
        public DateTime Date{ get; set; }
         public string TimeSlot { get; set; }
        public int MaxAppointments { get; set; }

    }
}