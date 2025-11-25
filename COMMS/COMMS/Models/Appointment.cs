using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COMMS.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int ScheduleID { get; set; }
        public string Status { get; set; } = "已预约";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}