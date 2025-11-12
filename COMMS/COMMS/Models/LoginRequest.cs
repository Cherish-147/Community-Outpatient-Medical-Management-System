using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace COMMS.Models
{
    public class LoginRequest
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}