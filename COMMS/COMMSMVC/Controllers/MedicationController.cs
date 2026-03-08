using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace COMMSMVC.Controllers
{
    public class MedicationController : Controller
    {
        #region 药品管理控制器
        private readonly string baseUrl = "https://localhost:7190/api";
        private HttpClient httpclient = new();
        public async Task<IActionResult> Index()
        {
            // 权限验证：仅管理员/医院工作人员可访问
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
            (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Doctor"))
            {
                return RedirectToAction("Login", "Home");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                //token 为空
                return RedirectToAction("Login", "Home");
            }
            var token = HttpContext.Session.GetString("JwtToken");
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var medicationsResult = await client.GetAsync(baseUrl + "/Medication/GetMedications");
            var medicationsResBody = await medicationsResult.Content.ReadAsStringAsync();
            var medications = JsonConvert.DeserializeObject<List<Medications>>(medicationsResBody);

            return View(medications);

        }
           
        
       
        #endregion
    }
}
