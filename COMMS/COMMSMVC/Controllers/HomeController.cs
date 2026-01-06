using COMMSMVC.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace COMMSMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult LoginJS()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        private readonly string baseUrl = "https://localhost:7190/api";
        [HttpPost]
        public async Task<IActionResult> Login(COMMSMVC.Models.LoginRequest model)
        {
            using HttpClient httpclient = new(); 
            var json = JsonSerializer.Serialize(model); 
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //var jsonContent = new StringContent(Isonserializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await httpclient.PostAsync(baseUrl + "/Home/Login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
                string token = tokenResponse?.token;
                ViewBag.Token = token;
                HttpContext.Session.SetString("JwtToken", tokenResponse!.token);
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var tokenRes = await httpclient.GetAsync(baseUrl + "/Home/GetUserTokenInfo");
                var tokenResBody = await tokenRes.Content.ReadAsStringAsync();
              
                var tokenInfo = JsonSerializer.Deserialize<TokenInfoRes>(tokenResBody);
                //存到Session
                HttpContext.Session.SetString("UserID",tokenInfo.UserID );
                HttpContext.Session.SetString("Username",tokenInfo.UserName );
                HttpContext.Session.SetString("JwtToken", tokenInfo.JWTToken);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "用户名或密码错误";
            return View(model);
        }
    }
}
