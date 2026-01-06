using System.ComponentModel.DataAnnotations;

namespace COMMSWebAPI.Models
{
    public class Schedule
    {
        [Key]
        public int ScheduleID { get; set; }
        public int DoctorID     { get; set; }

        public DateTime Date {  get; set; }
        
        public string TimeSlot { get; set; }

        public int MaxAppointments { get; set; }
    }
}
