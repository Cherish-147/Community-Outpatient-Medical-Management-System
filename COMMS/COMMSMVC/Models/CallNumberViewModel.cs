namespace COMMSMVC.Models
{
    public class CallNumberViewModel
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public int ScheduleID { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Remark { get; set; }
        public string TimeSlot { get; set; }
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }
    }
}
