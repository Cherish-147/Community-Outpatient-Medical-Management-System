
namespace COMMSMVC.Models
{
    public class CheckOrdersIndex
    {
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public string CheckItemName { get; set; }
        public int CheckOrderID { get; set; }
        public int AppointmentID { get; set; }
        public int CheckItemID { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
