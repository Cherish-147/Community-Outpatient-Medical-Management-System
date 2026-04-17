namespace COMMSMVC.Models
{
    public class EditPatientModel
    {
        public bool IsSuccess { get; set; }
        public string ?Message { get; set; }

        //public int PatientID { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public string Gender { get; set; }
        public string IDCard { get; set; }
        public string Phone { get; set; }
        public string InsuranceNo { get; set; }
        //public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsMarried { get; set; }
        public string Nation { get; set; }
        public string WorkUnit { get; set; }
        public string Occupation { get; set; }
        public string Address { get; set; }
        public string PastMedicalHistory { get; set; }
        public string DrugAllergyHistory { get; set; }
        public string GuardianName { get; set; }
        public string GuardianRelationship { get; set; }
        public string GuardianAddress { get; set; }
        public string GuardianPhone { get; set; }
        public string Remark { get; set; }

    }
}
