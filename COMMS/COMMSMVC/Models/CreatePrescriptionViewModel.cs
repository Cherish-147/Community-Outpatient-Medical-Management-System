using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMMSMVC.Models
{
    public class CreatePrescriptionViewModel
    {
        // 主表字段
        public int AppointmentID { get; set; }

        // 明细表字段
        public int MedicationID { get; set; }
        public decimal DoseValue { get; set; }
        public string DoseUnit { get; set; }
        public int Quantity { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string Remarks { get; set; }

        // 辅助属性（用于下拉列表）
        public List<SelectListItem> ?Appointments { get; set; }
        public List<SelectListItem> ?Medications { get; set; }
    }
}
