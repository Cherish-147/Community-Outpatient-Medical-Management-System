namespace COMMSMVC.Models
{
    public class PrescriptionDetailViewModel
    {
        public int PrescriptionID { get; set; }
        public int AppointmentID { get; set; }
        public string PatientName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PrescriptionDetailItem> Details { get; set; }
    }
    public class PrescriptionDetailItem
    {
        public int DetailID { get; set; }
        public string MedicationName { get; set; }
        public decimal DoseValue { get; set; }
        public string DoseUnit { get; set; }
        public int Quantity { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string Remarks { get; set; }
    }

}
