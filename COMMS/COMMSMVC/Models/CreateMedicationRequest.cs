using System.ComponentModel.DataAnnotations;

namespace COMMSMVC.Models
{
    public class CreateMedicationRequest
    {
        [Required(ErrorMessage = "药品名称不能为空")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "规格不能为空")]
        [StringLength(50)]
        public string Specification { get; set; }

        [Required(ErrorMessage = "价格不能为空")]
        [Range(0, double.MaxValue, ErrorMessage = "价格必须大于等于0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "库存不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "库存必须大于等于0")]
        public int Stock { get; set; }
        public bool IsActive { get; set; } 
    }
}
