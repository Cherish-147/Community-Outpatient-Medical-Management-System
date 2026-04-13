namespace COMMSMVC.Models
{
    public class CreateCheckOrderModel
    {

        public int AppointmentID { get; set; }
        public int CheckItemID { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
