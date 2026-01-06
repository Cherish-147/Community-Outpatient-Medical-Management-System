using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace COMMSWebAPI.Service

{
    public class AuthService:IAuthService
    {
        public string GenerateJwtToken(string username, string userId)
        {
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey12345"));
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASuperLongSecretKeyForJWTAuthentication123456789"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim("userId", userId),
                       new Claim(ClaimTypes.Name, username)};

            var token = new JwtSecurityToken(
                issuer: "qianfaren",//sender
                audience: "jieshouren",//reciver
                claims: claims,//声明,存放用户相关信息，比如userId,username
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials:credentials//签名凭证
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //哈希加密
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));// 使用十六进制格式，每个字节2个字符
                }
                return sb.ToString();
            }
            ;
        }
    }

}
