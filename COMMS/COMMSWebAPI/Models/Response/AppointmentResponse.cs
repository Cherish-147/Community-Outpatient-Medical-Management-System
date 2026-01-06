namespace COMMSWebAPI.Models.Response
{
    public class AppointmentResponse
    {
        public List<Departments> Departments { get; set; }
        public List<Doctor> Doctors { get; set; }
        public List<Schedule> Schedules { get; set; }
    }
}
