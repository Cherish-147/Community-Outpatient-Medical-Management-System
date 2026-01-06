using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMMSWebAPI.Models
{
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int ScheduleID { get; set; }
        public string Status { get; set; } = "已颈约"; 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
