using COMMSMVC.Models;
using COMMSMVC.Properties.Configurations;
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
        //private  string baseUrl = "https://localhost:7190/api";
        private string baseUrl = AppConfig.BaseUrl; // 直接静态调用
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
                var response = await client.PostAsync(baseUrl+"/Medication/CreateMedications", content);

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 权限验证
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

            // 调用 API 获取当前药品信息
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync($"{baseUrl}/Medication/GetMedicationById?medicationId={id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<Medications>>(json);
                    var medication = list.FirstOrDefault();
                    if (medication != null)
                    {
                        return View(medication);
                    }
                }

                // 未找到或失败
                TempData["Error"] = "未找到该药品或获取失败";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"请求异常：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        //更新
        [HttpPost]
        
        public async Task<IActionResult> Edit(int id, Medications model)
        {
            // 确保路径中的 id 与模型中的 MedicationID 一致
            if (id != model.MedicationID)
            {
                return BadRequest("ID 不匹配");
            }

            // 权限验证
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

            // 模型验证
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 准备调用 API 更新
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 构建请求体（包含需要更新的所有字段，createdAt 可能保留原值，updatedAt 由后端自动更新）
            var requestData = new
            {
                medicationID = model.MedicationID,
                name = model.Name,
                specification = model.Specification,
                price = model.Price,
                stock = model.Stock,
                createdAt = model.CreatedAt,      // 保留原创建时间（如果 API 需要）
                updatedAt = DateTime.Now,          // 或由后端自动生成，这里可以忽略
                isActive = model.IsActive
            };

            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // 注意 API 地址：UpdateMedications{medicationId}，可能需要拼接 id
                var response = await client.PostAsync($"{baseUrl}/Medication/UpdateMedications?medicationId={id}", content);
                // 如果 API 使用 POST 或 PUT，请根据实际情况选择 PutAsync 或 PostAsync

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "药品信息更新成功！";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"更新失败：{response.StatusCode} - {error}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"请求异常：{ex.Message}");
                return View(model);
            }
        }

        //删除
        [HttpGet]
        public async Task<IActionResult> DeleteGet(int id)
        {            // 权限验证
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

            // 调用 API 获取当前药品信息
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync($"{baseUrl}/Medication/GetMedicationById?medicationId={id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<Medications>>(json);
                    var medication = list.FirstOrDefault();
                    if (medication != null)
                    {
                        return View(medication);
                    }
                }

                // 未找到或失败
                TempData["Error"] = "未找到该药品或获取失败";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"请求异常：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            
            // 权限验证
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


            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await client.PostAsync($"{baseUrl}/Medication/DeleteMedicationById?medicationId={id}",null);


                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "药品删除成功！";
                }
                else
                {
                    // 可以读取响应内容获取详细错误信息
                    string error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"删除失败：{response.StatusCode} - {error}";
                }
            }
            catch (HttpRequestException ex)
            {

                TempData["ErrorMessage"] = $"网络请求异常：{ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"系统错误：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));

        }


        #endregion
    }
}
