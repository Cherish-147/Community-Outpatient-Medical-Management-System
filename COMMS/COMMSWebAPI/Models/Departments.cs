using System.ComponentModel.DataAnnotations;

namespace COMMSWebAPI.Models
{
    public class Departments
    {
        [Key]
        public int DeptID { get; set; }
        public string DeptName { get; set; }

        public string DeptDescription { get; set;}
    }
}
