using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Text;

namespace COMMSMVC.Controllers
{
    public class MedicationController : Controller
    {
        #region 药品管理控制器
        private string baseUrl = "https://localhost:7190/api";
        private string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        private HttpClient httpclient = new();
        public MedicationController(IOptions<ApiConfig> apiConfig, IConfiguration configuration)
        {
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
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
                var response = await client.PostAsync($"{baseUrl}/Medication/CreateMedications", content);

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
                var response = await client.PostAsync($"{baseUrl}/Medication/DeleteMedicationById?medicationId={id}", null);


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

        //public virtual void CreateDispenseMedication(int?appointmentID)
        //{

        //}
        //public virtual UpdateMedicationStockModel UpdateMedicationStock(int MedicationID,int? appointmentID)
        //{
        //    var updateResponse = new UpdateMedicationStockModel
        //    {
        //        IsSuccess = false,
        //        Message = $"更新{MedicationID}药品库存失败"
        //    };
        //    string sql = @"update Medications set Stock =@Stock where MedicationID =@MedicalRecords";

        //    if (!appointmentID.HasValue)
        //    {
        //        updateResponse.Message = "预约ID不能为空";
        //        return updateResponse;
        //    }

        //    // 1. 查询该处方中指定药品的用量
        //    string getQuantitySql = @"
        //SELECT pd.Quantity
        //FROM PrescriptionDetails pd
        //INNER JOIN Prescriptions p ON pd.PrescriptionID = p.PrescriptionID
        //WHERE p.AppointmentID = @AppointmentID AND pd.MedicationID = @MedicationID";

        //    int quantity = 0;
        //    using (SqlConnection conn = new SqlConnection(_connectionString))
        //    using (SqlCommand cmd = new SqlCommand(getQuantitySql, conn))
        //    {
        //        cmd.Parameters.AddWithValue("@AppointmentID", appointmentID.Value);
        //        cmd.Parameters.AddWithValue("@MedicationID", MedicationID);
        //        conn.Open();
        //        object result = cmd.ExecuteScalar();
        //        if (result == null || result == DBNull.Value)
        //        {
        //            updateResponse.Message = $"未找到预约 {appointmentID} 中药品 {MedicationID} 的用量信息";
        //            return updateResponse;
        //        }
        //        quantity = Convert.ToInt32(result);
        //    }

        //    // 2. 更新库存（扣减用量）
        //    string updateStockSql = @"
        //UPDATE Medications
        //SET Stock = Stock - @Quantity
        //WHERE MedicationID = @MedicationID AND Stock >= @Quantity";

        //    using (SqlConnection conn = new SqlConnection(_connectionString))
        //    using (SqlCommand cmd = new SqlCommand(updateStockSql, conn))
        //    {
        //        cmd.Parameters.AddWithValue("@Quantity", quantity);
        //        cmd.Parameters.AddWithValue("@MedicationID", MedicationID);
        //        conn.Open();
        //        int rowsAffected = cmd.ExecuteNonQuery();
        //        if (rowsAffected > 0)
        //        {
        //            updateResponse.IsSuccess = true;
        //            updateResponse.Message = $"药品 {MedicationID} 库存扣减 {quantity} 成功";
        //        }
        //        else
        //        {
        //            updateResponse.Message = $"药品 {MedicationID} 库存不足或更新失败";
        //        }
        //    }
        //    return updateResponse;
        //}

        #region 发药
        [HttpGet]
        public async Task<IActionResult> CheckDispenseMedication(int ?id)// id = AppointmentID
        {
            var checkModel = await GetDispenseMedicationInfo(id);

            return View(checkModel);
        }
        public virtual async Task<List<CheckDispenseMedicationModel>> GetDispenseMedicationInfo(int? appointmentID)
        {
            var checkDispenseMedicationList = new List<CheckDispenseMedicationModel>();
            string sql = @"
                            SELECT
                                -- 患者信息
                                p.PatientID,
                                p.Name       AS PatientName,
                                a.AppointmentID,
                                pr.PrescriptionID,
                            
                                -- 药品信息
	                            pd.DetailID,
                                m.MedicationID,
                                m.Name       AS MedicationName,
                                pd.Quantity  AS Quantity,
                                m.Stock      AS Stock,
                                pd.Remarks   AS Remarks
                            
                            FROM Prescriptions pr
                            INNER JOIN Appointments a      ON pr.AppointmentID = a.AppointmentID
                            INNER JOIN Patients p          ON a.PatientID       = p.PatientID
                            INNER JOIN PrescriptionDetails pd ON pr.PrescriptionID = pd.PrescriptionID
                            INNER JOIN Medications m       ON pd.MedicationID   = m.MedicationID
                            
                            WHERE
                                a.AppointmentID = @AppointmentID
                                AND pd.Remarks <> '已发药'                     -- 只查未发药
                                AND m.MedicationID IS NOT NULL                 -- 有药品
                                AND m.Name IS NOT NULL                         -- 药名不为空
                                AND pd.Quantity IS NOT NULL                    -- 数量不为空
                            
                            ORDER BY pd.DetailID
                            ";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 处理可能为空的 appointmentID
                    //cmd.Parameters.AddWithValue("@AppointmentID", appointmentID ?? (object)DBNull.Value);
                    cmd.Parameters.Add("@AppointmentID", SqlDbType.Int).Value = appointmentID.HasValue ? appointmentID.Value : DBNull.Value;
                    await conn.OpenAsync();

                    using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var model = new CheckDispenseMedicationModel
                        {
                            IsSuccess = true,
                            Message = null,
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                            DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                            MedicationID = reader.GetInt32(reader.GetOrdinal("MedicationID")),
                            MedicationName = reader.GetString(reader.GetOrdinal("MedicationName")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks"))
                        };
                        checkDispenseMedicationList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                // 可根据需要记录日志（例如使用 ILogger）
                // 此处返回空列表，避免调用方出错
                string msg = ex.Message;
                checkDispenseMedicationList.Add(new CheckDispenseMedicationModel
                {
                    IsSuccess = false,
                    Message = msg // 或自定义错误消息
                });
            }
            return checkDispenseMedicationList;
        }

        //发药
        [HttpPost]
        public async Task<IActionResult> Dispense(int id)//int apponitmentID
        {
            // 🔥 关键：SQL 里 不声明 declare @AppointmentID
            // 🔥 直接使用 C# 传进去的 @AppointmentID
            string sql = @"
--declare @AppointmentID int;
declare @IsSuccess bit ;
declare @Message Nvarchar(Max);
BEGIN TRANSACTION; -- 开始事务

BEGIN TRY
	set @IsSuccess=0
    -- 1. 更新药品库存：Stock = Stock - 已开药的数量
    UPDATE m
    SET m.Stock = m.Stock - pd.Quantity
    FROM Medications m
    INNER JOIN PrescriptionDetails pd ON m.MedicationID = pd.MedicationID
    INNER JOIN Prescriptions pr ON pd.PrescriptionID = pr.PrescriptionID
    INNER JOIN Appointments a ON pr.AppointmentID = a.AppointmentID
	INNER JOIN Patients p ON a.PatientID = p.PatientID
    WHERE 
        a.AppointmentID = @AppointmentID 
        AND m.MedicationID IS NOT NULL
        AND m.Name IS NOT NULL
        AND pd.Quantity IS NOT NULL
        AND pd.Remarks <> '已发药';
		


    -- 2. 更新处方明细状态为：已发药
    UPDATE pd
    SET pd.Remarks = '已发药'
    FROM PrescriptionDetails pd
    INNER JOIN Prescriptions pr ON pd.PrescriptionID = pr.PrescriptionID
    INNER JOIN Appointments a ON pr.AppointmentID = a.AppointmentID
	INNER JOIN Patients p ON a.PatientID = p.PatientID
    WHERE 
        a.AppointmentID = @AppointmentID
        AND pd.Remarks <> '已发药';



    COMMIT TRANSACTION; -- 全部成功，提交
	set @IsSuccess= 1;
    set @Message = '发药成功：库存已扣减，状态已更新';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION; -- 出错，回滚（库存不变，状态不变）
    set @Message = '发药失败：已回滚';
	  SET @Message = '发药失败：已回滚。错误信息：' + ERROR_MESSAGE();
    --THROW; -- 抛出错误信息
END CATCH
-- 输出结果（给你看成功还是失败）
SELECT 
    @IsSuccess AS IsSuccess,
    @Message AS Message;
         
";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@AppointmentID", SqlDbType.Int).Value = id;

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                            string message = reader.GetString(reader.GetOrdinal("Message"));

                            if (isSuccess)
                                TempData["SuccessMessage"] = message;
                            else
                                TempData["ErrorMessage"] = message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"发药操作异常：{ex.Message}";
            }

            // 发药后重定向回待发药清单页面
            return RedirectToAction(nameof(CheckDispenseMedication), new { id = id });
          
        }
        #endregion
    }
}
