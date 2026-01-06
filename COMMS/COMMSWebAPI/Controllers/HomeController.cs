using COMMSWebAPI.Models;
using COMMSWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COMMSWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private MyDbContext db;
        public IHomeService iHomeService;
        public IAuthService iAuthService;
        public HomeController(MyDbContext _db, IHomeService _iHomeService, IAuthService _iAuthService)
        {
            db = _db;
            iHomeService = _iHomeService;
            iAuthService = _iAuthService;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult>LoginBackUp(Models.Request.LoginRequest loginRequset)
        {
            string storePassword;
            var login = await db.Users.FirstOrDefaultAsync(u => u.UserName == loginRequset.Username);
            if (login == null)
            {
                return NotFound();
            }
            storePassword= login.Password;
            string userid =login.UserID.ToString();
            string hashedInputPassword = iAuthService.HashPassword(loginRequset.Password);
            var token = iAuthService.GenerateJwtToken(loginRequset.Username, userid);
            if(storePassword == hashedInputPassword)
            {
                return Ok(new { token });
            }
            return Unauthorized(new { message="Invalid username or password"});
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]Models.Request.LoginRequest loginRequset)
        {
            if (loginRequset == null) { return NotFound(); }
            var token =await iHomeService.LoginAsync(loginRequset);
            if (token==null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            return Ok(new { token });
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetUserId()
        {
            var userId = User.FindFirst("userId")?.Value;
            //var username = User.FindFirst(ClaimTypes.Name)?.Value;
            //var token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            return Ok(new { userId });
        }
        [HttpGet]
        public IActionResult GetUserTokenInfo()
        {
            var userId = User.FindFirst("userId")?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            return Ok(new { userId, username, token });
        }
    }
}
