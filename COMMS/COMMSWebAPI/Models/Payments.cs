
using System.ComponentModel.DataAnnotations;

namespace COMMSWebAPI.Models
{
    public class Payments
    {
        [Key]
        public int PaymentID {  get; set; }
        public int AppointmentID {  get; set; }
        public decimal Amount { get; set; }

        public string Method { get; set; }//支付方式，支付宝。微信。医保...

        public string Status { get; set; }//已支付未支付

        public DateTime? PaidAt { get; set; }
    }
}
