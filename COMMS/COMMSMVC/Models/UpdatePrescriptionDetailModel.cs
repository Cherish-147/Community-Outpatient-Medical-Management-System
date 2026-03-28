using System.ComponentModel.DataAnnotations;

namespace COMMSMVC.Models
{
    public class UpdatePrescriptionDetailModel
    {
        [Required]
        public int DetailID { get; set; }

        [Required(ErrorMessage = "请选择药品")]
        public int MedicationID { get; set; }

        [Required(ErrorMessage = "请输入剂量值")]
        [Range(0.01, double.MaxValue, ErrorMessage = "剂量值必须大于0")]
        public decimal DoseValue { get; set; }

        public string DoseUnit { get; set; }

        [Required(ErrorMessage = "请输入数量")]
        [Range(1, int.MaxValue, ErrorMessage = "数量必须大于0")]
        public int Quantity { get; set; }

        public string Frequency { get; set; }

        [Range(0, 365, ErrorMessage = "疗程天数应在0-365之间")]
        public int Duration { get; set; }

        public string Remarks { get; set; }
    }
}
