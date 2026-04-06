using System.ComponentModel.DataAnnotations;

namespace COMMSMVC.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int AppointmentID { get; set; }

        public int PatientID { get; set; }
        public string PatientName { get; set; }
        [Required(ErrorMessage = "请输入金额")]
        [Range(0.01, double.MaxValue, ErrorMessage = "金额必须大于0")]
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }

        public DateTime PaidAt { get; set; }
    }
}
