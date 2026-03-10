namespace COMMSMVC.Models
{
    public class CheckOrder
    {
        public int CheckOrderID { get; set; }
        public int AppointmentID { get; set; }
        public int CheckItemID { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
