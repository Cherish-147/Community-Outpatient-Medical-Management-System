using System.ComponentModel.DataAnnotations;

namespace COMMSMVC.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "用户名不能为空")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "请再次输入密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string PasswordAgain { get; set; }

        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Gender { get; set; }

    }
}
