using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace COMMSMVC.Controllers
{
    public class MedicationController : Controller
    {
        #region 药品管理控制器
        private readonly string baseUrl = "https://localhost:7190/api";
        private HttpClient httpclient = new();
        //药品管理首页
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
        //新增药品
        [HttpGet]
        public IActionResult Create()
        {
            // 验证权限
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateMedicationRequest createMedicationRequest)
        {
            #region 验证
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
       (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Doctor"))
            {
                return RedirectToAction("Login", "Home");
            }

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }
            #endregion
            // 2. 模型验证
            if (!ModelState.IsValid)
            {
                // 如果验证失败，返回当前视图并显示错误
                return View(createMedicationRequest);
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestData = new
            {
                name = createMedicationRequest.Name,
                specification = createMedicationRequest.Specification,
                price = createMedicationRequest.Price,
                stock = createMedicationRequest.Stock
            };
            // 序列化为 JSON
            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                // 4. 发送 POST 请求
                var response = await client.PostAsync("https://localhost:7190/api/Medication/CreateMedications", content);

                if (response.IsSuccessStatusCode)
                {
                    // 成功：跳转到列表页
                    TempData["Success"] = "药品添加成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // 失败：读取错误信息并显示
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"API 调用失败：{response.StatusCode} - {error}");
                    return View(createMedicationRequest);
                }
            }
            catch (Exception ex)
            {
                // 网络异常等
                ModelState.AddModelError(string.Empty, $"请求异常：{ex.Message}");
                return View(createMedicationRequest);
            }
          
        }



        #endregion
    }
}
