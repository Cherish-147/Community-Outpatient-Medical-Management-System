using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace COMMS.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required ]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
    }
}