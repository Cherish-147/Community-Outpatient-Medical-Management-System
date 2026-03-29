namespace COMMSMVC.Models
{
    public class DetailPatientModel:Patient
    {
        public DetailPatientModel():base() { }

        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
