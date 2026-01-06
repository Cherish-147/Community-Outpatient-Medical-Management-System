using COMMSWebAPI.Models;
using COMMSWebAPI.Models.Request;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace COMMSWebAPI.Service
{
    public class HomeService:IHomeService
    {
        private readonly MyDbContext db;
        private readonly  IAuthService iAuthService;
        public HomeService(MyDbContext _db,IAuthService _iAuthService) {
            db = _db;
            iAuthService = _iAuthService;
            //IHomeService = _iHomeService;//Home Service 不能调用自己抽象方法不然会循环引用
                
        }

        public async Task<string?> LoginAsync(Models.Request.LoginRequest loginRequest)
        {

            string storePassword;
            var login = await db.Users.FirstOrDefaultAsync(u => u.UserName == loginRequest.Username);
            if (login == null)
            {
                return null;
            }
            storePassword = login.Password;
            string userid = login.UserID.ToString();
            string hashedInputPassword = iAuthService.HashPassword(loginRequest.Password);
            var token = iAuthService.GenerateJwtToken(loginRequest.Username, userid);
            if (storePassword == hashedInputPassword)
            {
                return token;
            }
            return null;
        }
    }
}
