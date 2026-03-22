namespace COMMSMVC.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int AppointmentID { get; set; }

        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }

        public DateTime PaidAt { get; set; }
    }
}
