namespace COMMSMVC.Models
{
    public class MedicalRecord
    {
        public int RecordID { get; set; }
        public int AppointmentID { get; set; }
        public string? PatientStatement { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
