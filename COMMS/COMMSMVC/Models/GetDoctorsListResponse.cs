namespace COMMSMVC.Models
{
    public class GetDoctorsListResponse
    {
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }
        public string Title { get; set; }
        public int? DeptID { get; set; }        // 注意可空，因为左连接可能无对应科室
        public string DeptName { get; set; }    // 可能为 null
        public string Phone { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
