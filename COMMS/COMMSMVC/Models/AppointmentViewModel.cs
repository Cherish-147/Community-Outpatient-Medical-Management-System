namespace COMMSMVC.Models
{
    public class AppointmentViewModel
    {

            public int AppointmentID { get; set; }
            public int PatientID { get; set; }
            public string PatientName { get; set; }  // 对应数据中的 Name 字段
            public int ScheduleID { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Remark { get; set; }
        

    }
}
