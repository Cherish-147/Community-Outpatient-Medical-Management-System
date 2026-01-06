using System.ComponentModel.DataAnnotations;

namespace COMMSWebAPI.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }
        public string Title { get; set; }//职务。主任副主任等等
        public string DoctorPhone {  get; set; }
        public int DeptID { get; set; }//关联部门Id

        public string DoctorDescription {  get; set; }//医生简介
    }
}
