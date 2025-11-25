using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace COMMS.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int DeptID { get; set; }
    }
}