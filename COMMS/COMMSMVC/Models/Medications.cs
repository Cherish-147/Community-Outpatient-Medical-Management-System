using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMMSMVC.Models
{
    [Table("Medications")]
    public class Medications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicationID { get; set; }//自增id 主键

        public string Name { get; set; }//药名

        public string Specification { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; }

    }
}
