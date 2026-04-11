using COMMSMVC.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;


namespace COMMSMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private string baseUrl = "https://192.168.5.6/api";
        private  string baseUrl = "https://localhost:7190/api";
        private string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        public HomeController(ILogger<HomeController> logger, IOptions<ApiConfig> apiConfig, IConfiguration configuration)
        {
            _logger = logger;
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
         

        
           

         
       

        public IActionResult Index()
        {
            // 从数据库查询今日挂号数
            //ViewData["TodayRegistrationCount"] = _registrationService.GetTodayCount();
            // 其他数据查询...
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Doctor"))
            {
                return RedirectToAction("Login", "Home");
            }
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
        [HttpGet]
        public IActionResult Register()
        {
            return View();

        }
        [HttpPost]
        public IActionResult Register(Users user)
        {
           return View();
        }

        //IIS 不支持双斜杠 //
        //IIS Express 支持
        [HttpPost]
        public async Task<IActionResult> Login(COMMSMVC.Models.LoginRequest model)
        {

            if (model.Username == null)//自动输入密码管理员，后面要删，方便测试
            {
                model.Username = "Admin";
                model.Password = "123";
            }
            else if (model.Username == "蔡文姬")

            {
                model.Password = "2";
            }
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
                HttpContext.Session.SetString("Role",tokenInfo.Role );
                HttpContext.Session.SetString("JwtToken", tokenInfo.JWTToken);
                
               
                if (HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "Doctor")
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (HttpContext.Session.GetString("Role") == "Patient")
                {
                    int patientId = await GetPatientIDByUserId(int.Parse(tokenInfo.UserID));

                    HttpContext.Session.SetInt32("patientId", patientId);
                    //int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));

                    return RedirectToAction("Register", "Patient");
                }
            }
            ViewBag.Error = "用户名或密码错误";
            return View(model);
        }
      
        public virtual async Task<int> GetPatientIDByUserId(int userId)
        {
            string sql = @"SELECT PatientID FROM Patients WHERE UserId = @UserId";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    await conn.OpenAsync();
                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1; // 未找到对应患者，返回-1
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "根据UserId获取PatientID失败，UserId: {UserId}", userId);
                return 0; // 或重新抛出异常，根据业务需求决定
            }
        }
        public IActionResult Logout()
        {
            // 1. 清空所有用户相关的Session数据（关键）
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.Remove("UserID");
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("Role");

            // 可选：清空所有Session（如果不需要保留其他Session数据）
             HttpContext.Session.Clear();

            // 2. 跳转到登录页（可根据需求调整跳转目标）
            return RedirectToAction("Login", "Home");
        }
    }
}
