namespace COMMSMVC.Models
{
    public class PrescriptionsIndexModel
    {
        public int PrescriptionID { get; set; }          // 处方ID
        public int AppointmentID { get; set; }          // 预约ID
        public DateTime CreatedAt { get; set; }         // 创建时间
        public int DetailID { get; set; }               // 明细ID
        public int MedicationID { get; set; }           // 药品ID
        public decimal DoseValue { get; set; }          // 剂量值
        public string DoseUnit { get; set; }            // 剂量单位
        public int Quantity { get; set; }               // 数量
        public string Frequency { get; set; }           // 频次
        public int Duration { get; set; }            // 持续时长(int)
        public string PatientName { get; set; }         // 患者姓名
        public string MedicationName { get; set; }      // 药品名称
    }
}
