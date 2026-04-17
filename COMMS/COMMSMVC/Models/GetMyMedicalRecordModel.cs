namespace COMMSMVC.Models
{
    public class GetMyMedicalRecordModel : MedicalRecord
    {

        public GetMyMedicalRecordModel() : base()
        {
        }
        public string? PatientName { get; set; }

    }
}
