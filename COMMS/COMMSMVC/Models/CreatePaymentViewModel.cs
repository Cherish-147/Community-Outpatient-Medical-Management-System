using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMMSMVC.Models
{
    public class CreatePaymentViewModel
    {
        public Payment ?Payment { get; set; }
        public SelectList? AppointmentList { get; set; }
    }
}
