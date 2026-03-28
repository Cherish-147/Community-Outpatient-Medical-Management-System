using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMMSMVC.Models
{
    public class EditPrescriptionDetailViewModel
    {
        // 明细主键
        public int DetailID { get; set; }

        // 处方主表关联信息（只读显示）
        public int PrescriptionID { get; set; }
        public string PatientName { get; set; }

        // 药品信息（只读显示药品名称，如果需要可改为下拉框，但通常处方已开，药品不再更改）
        public int MedicationID { get; set; }
        public string MedicationName { get; set; }// 可用于展示当前药品名称，但下拉框会回显

        // 可编辑字段
        public decimal DoseValue { get; set; }
        public string DoseUnit { get; set; }
        public int Quantity { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string Remarks { get; set; }

        // 可选：药品下拉列表（若允许修改药品）
        public List<SelectListItem> Medications { get; set; }// 用于下拉框
    }
}
