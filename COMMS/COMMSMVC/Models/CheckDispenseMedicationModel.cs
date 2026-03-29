namespace COMMSMVC.Models
{
    public class CheckDispenseMedicationModel
    {
        public bool IsSuccess { get; set; }//是否成功
        public string? Message { get; set; }//提示信息
        public int ?PatientID { get; set; }
        public string?PatientName  { get; set; }//患者姓名
        public int? AppointmentID { get; set; }
        public int? PrescriptionID { get; set; }
        public int? DetailID { get; set; }
        public int? MedicationID { get; set; }
        public string? MedicationName { get; set; }//药品名称
        public int? Quantity { get; set; }//发药数量
        public int? Stock { get; set; }//当前库存
        public string? Remarks { get; set; }//发药状态Remarks
    }
}
