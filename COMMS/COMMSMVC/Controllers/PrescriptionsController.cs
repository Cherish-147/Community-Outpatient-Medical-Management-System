using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using System.Text;
using System.Transactions;

namespace COMMSMVC.Controllers
{
    public class PrescriptionsController : Controller
    {
        #region 开处方控制器
        private readonly ILogger<PrescriptionsController> _logger;
        //private string baseUrl = "https://localhost:7190/api";
        private string baseUrl;
        //private string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        private string _connectionString;
        public PrescriptionsController(ILogger<PrescriptionsController> logger, IOptions<ApiConfig> apiConfig, IConfiguration configuration)
        {
            _logger = logger;
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
 #endregion
        [HttpGet]
        public async Task<IActionResult> Index(int? prescriptionId, string patientName)//控制器，处方列表首页
        {
            List<PrescriptionsIndexModel> prescriptionsList = [];
            if (prescriptionId.HasValue || !string.IsNullOrWhiteSpace(patientName))
            {
                // 有搜索条件，调用带条件的方法
                prescriptionsList = await GetPrescriptionsIndexInfoAsync(prescriptionId, patientName);
                ViewBag.PrescriptionId = prescriptionId;
                ViewBag.PatientName = patientName;
            }
            else
            {
                // 无条件，获取全部
                prescriptionsList = await GetPrescriptionsIndexInfoAsync();
            }
            return View(prescriptionsList);
        }

        public virtual async Task<List<PrescriptionsIndexModel>> GetPrescriptionsIndexInfoAsync()//方法，获取处方列表信息
        {
            var prescriptionsList = new List<PrescriptionsIndexModel>();
            string sql = @"
                            select pr.PrescriptionID,pr.AppointmentID,pr.CreatedAt
                            ,prd.DetailID
                            ,prd.MedicationID
                            ,prd.DoseValue
                            ,prd.DoseUnit
                            ,prd.Quantity
                            ,prd.Frequency
                            ,prd.Duration
                            ,p.Name as patientName
                            ,m.Name as medicationName
                            from Prescriptions pr
                            inner join PrescriptionDetails prd on pr.PrescriptionID =prd.PrescriptionID
                            inner join Appointments a on  pr.AppointmentID =a.AppointmentID
                            inner join Patients p on a.PatientID =p.PatientID
                            inner join Medications m on prd.MedicationID=m.MedicationID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var model = new PrescriptionsIndexModel
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                                MedicationID = reader.GetInt32(reader.GetOrdinal("MedicationID")),
                                DoseValue = reader.GetDecimal(reader.GetOrdinal("DoseValue")),
                                DoseUnit = reader.IsDBNull(reader.GetOrdinal("DoseUnit")) ? null : reader.GetString(reader.GetOrdinal("DoseUnit")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Frequency = reader.IsDBNull(reader.GetOrdinal("Frequency")) ? null : reader.GetString(reader.GetOrdinal("Frequency")),
                                Duration = (int)(reader.IsDBNull(reader.GetOrdinal("Duration")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Duration"))),
                                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                MedicationName = reader.GetString(reader.GetOrdinal("MedicationName"))
                            };
                            prescriptionsList.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志（建议使用 ILogger）
                Console.WriteLine($"获取处方列表失败：{ex.Message}");
                // 根据需要可以重新抛出或返回空列表
                return null;
            }
            return prescriptionsList;
        }
        public virtual async Task<List<PrescriptionsIndexModel>> GetPrescriptionsIndexInfoAsync(int? prescriptionId, string patientName)
        {
            var prescriptionsList = new List<PrescriptionsIndexModel>();

            // 构建动态 SQL
            StringBuilder sql = new StringBuilder(@"
        select pr.PrescriptionID, pr.AppointmentID, pr.CreatedAt,
               prd.DetailID, prd.MedicationID, prd.DoseValue, prd.DoseUnit,
               prd.Quantity, prd.Frequency, prd.Duration,
               p.Name as PatientName, m.Name as MedicationName
        from Prescriptions pr
        inner join PrescriptionDetails prd on pr.PrescriptionID = prd.PrescriptionID
        inner join Appointments a on pr.AppointmentID = a.AppointmentID
        inner join Patients p on a.PatientID = p.PatientID
        inner join Medications m on prd.MedicationID = m.MedicationID
        where 1=1");

            var parameters = new List<SqlParameter>();

            if (prescriptionId.HasValue)
            {
                sql.Append(" AND pr.PrescriptionID = @PrescriptionID");
                parameters.Add(new SqlParameter("@PrescriptionID", prescriptionId.Value));
            }

            if (!string.IsNullOrWhiteSpace(patientName))
            {
                sql.Append(" AND p.Name LIKE @PatientName");
                parameters.Add(new SqlParameter("@PatientName", $"%{patientName}%"));
            }

            // 可选排序
            sql.Append(" ORDER BY pr.PrescriptionID DESC");

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql.ToString(), conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var model = new PrescriptionsIndexModel
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                                MedicationID = reader.GetInt32(reader.GetOrdinal("MedicationID")),
                                DoseValue = reader.GetDecimal(reader.GetOrdinal("DoseValue")),
                                DoseUnit = reader.IsDBNull(reader.GetOrdinal("DoseUnit")) ? null : reader.GetString(reader.GetOrdinal("DoseUnit")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Frequency = reader.IsDBNull(reader.GetOrdinal("Frequency")) ? null : reader.GetString(reader.GetOrdinal("Frequency")),
                                Duration = (int)(reader.IsDBNull(reader.GetOrdinal("Duration")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Duration"))),
                                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                MedicationName = reader.GetString(reader.GetOrdinal("MedicationName"))
                            };
                            prescriptionsList.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                Console.WriteLine($"获取处方列表失败：{ex.Message}");
                return null;
            }
            return prescriptionsList;
        }

        // GET: /Prescriptions/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreatePrescriptionViewModel();

            // 获取可开处方的预约列表（状态为“已叫号”或“已就诊”）
            viewModel.Appointments = await GetAppointmentSelectListAsync();

            // 获取药品列表
            viewModel.Medications = await GetMedicationSelectListAsync();

            return View(viewModel);
        }
        // POST: /Prescription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"验证错误: {error.ErrorMessage}");
                }
                // 验证失败，重新加载下拉列表
                model.Appointments = await GetAppointmentSelectListAsync();
                model.Medications = await GetMedicationSelectListAsync();
                return View(model);
            }

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 1. 插入处方主表，获取新ID
                    int prescriptionId = await InsertPrescriptionAsync(model.AppointmentID);

                    // 2. 插入处方明细
                    await InsertPrescriptionDetailAsync(prescriptionId, model);

                    transaction.Complete();

                    TempData["SuccessMessage"] = $"处方创建成功！处方编号：{prescriptionId}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"创建失败：{ex.Message}");
                    model.Appointments = await GetAppointmentSelectListAsync();
                    model.Medications = await GetMedicationSelectListAsync();
                    return View(model);
                }
            }
        }
        // 辅助方法：获取可开处方的预约列表
        private async Task<List<SelectListItem>> GetAppointmentSelectListAsync()
        {
            var list = new List<SelectListItem>();
            string sql = @"
            SELECT a.AppointmentID, p.Name AS PatientName
            FROM Appointments a
            INNER JOIN Patients p ON a.PatientID = p.PatientID
            WHERE a.Status IN ('已叫号', '已就诊')
            ORDER BY a.AppointmentID DESC";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("AppointmentID")).ToString(),
                            Text = $"预约ID：{reader.GetInt32(reader.GetOrdinal("AppointmentID"))} - {reader.GetString(reader.GetOrdinal("PatientName"))}"
                        });
                    }
                }
            }
            return list;
        }

        // 辅助方法：获取药品列表
        private async Task<List<SelectListItem>> GetMedicationSelectListAsync()
        {
            var list = new List<SelectListItem>();
            string sql = "SELECT MedicationID, Name FROM Medications WHERE IsActive = 1 ORDER BY Name";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("MedicationID")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }
                }
            }
            return list;
        }

        // 插入主表并返回新ID
        private async Task<int> InsertPrescriptionAsync(int appointmentId)
        {
            string sql = @"
            INSERT INTO Prescriptions (AppointmentID)
            OUTPUT INSERTED.PrescriptionID
            VALUES (@AppointmentID)";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                await conn.OpenAsync();
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        // 插入明细
        private async Task InsertPrescriptionDetailAsync(int prescriptionId, CreatePrescriptionViewModel model)
        {
            string sql = @"
            INSERT INTO PrescriptionDetails 
            (PrescriptionID, MedicationID, DoseValue, DoseUnit, Quantity, Frequency, Duration, Remarks)
            VALUES (@PrescriptionID, @MedicationID, @DoseValue, @DoseUnit, @Quantity, @Frequency, @Duration, @Remarks)";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PrescriptionID", prescriptionId);
                cmd.Parameters.AddWithValue("@MedicationID", model.MedicationID);
                cmd.Parameters.AddWithValue("@DoseValue", model.DoseValue);
                cmd.Parameters.AddWithValue("@DoseUnit", model.DoseUnit ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                cmd.Parameters.AddWithValue("@Frequency", model.Frequency ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Duration", model.Duration);
                cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // GET: /Prescription/EditDetail/5
        [HttpGet]
        public async Task<IActionResult> EditDetail(int id)
        {
            var viewModel = await GetEditPrescriptionDetailViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            // 如果需要修改药品，加载药品下拉列表
            viewModel.Medications = await GetMedicationSelectListAsync();

            return View(viewModel);
        }

        // POST: /Prescription/EditDetail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetail(EditPrescriptionDetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 验证失败，重新加载药品下拉列表并返回视图
                model.Medications = await GetMedicationSelectListAsync();
                return View(model);
            }

            try
            {
                bool success = await UpdatePrescriptionDetailAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "处方明细更新成功！";
                    // 更新成功后重定向到处方详情页或列表页
                    return RedirectToAction("Details", new { id = model.PrescriptionID });
                }
                else
                {
                    ModelState.AddModelError("", "更新失败，请稍后重试。");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失败：{ex.Message}");
            }

            // 更新失败，重新加载数据并返回视图
            var freshViewModel = await GetEditPrescriptionDetailViewModelAsync(model.DetailID);
            if (freshViewModel != null)
            {
                model.PrescriptionID = freshViewModel.PrescriptionID;
                model.PatientName = freshViewModel.PatientName;
                model.MedicationName = freshViewModel.MedicationName;
                model.Medications = await GetMedicationSelectListAsync();
            }
            return View(model);
        }

        // 辅助方法：根据DetailID获取编辑视图模型
        private async Task<EditPrescriptionDetailViewModel> GetEditPrescriptionDetailViewModelAsync(int detailId)
        {
            string sql = @"
        SELECT pd.DetailID, pd.PrescriptionID, pd.MedicationID, pd.DoseValue, pd.DoseUnit,
               pd.Quantity, pd.Frequency, pd.Duration, pd.Remarks,
               m.Name AS MedicationName,
               p.Name AS PatientName
        FROM PrescriptionDetails pd
        INNER JOIN Prescriptions pr ON pd.PrescriptionID = pr.PrescriptionID
        INNER JOIN Appointments a ON pr.AppointmentID = a.AppointmentID
        INNER JOIN Patients p ON a.PatientID = p.PatientID
        LEFT JOIN Medications m ON pd.MedicationID = m.MedicationID
        WHERE pd.DetailID = @DetailID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@DetailID", detailId);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new EditPrescriptionDetailViewModel
                        {
                            DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                            PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                            MedicationID = reader.IsDBNull(reader.GetOrdinal("MedicationID")) ? 0 : reader.GetInt32(reader.GetOrdinal("MedicationID")),
                            MedicationName = reader.IsDBNull(reader.GetOrdinal("MedicationName")) ? "未知药品" : reader.GetString(reader.GetOrdinal("MedicationName")),
                            DoseValue = reader.GetDecimal(reader.GetOrdinal("DoseValue")),
                            DoseUnit = reader.IsDBNull(reader.GetOrdinal("DoseUnit")) ? null : reader.GetString(reader.GetOrdinal("DoseUnit")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Frequency = reader.IsDBNull(reader.GetOrdinal("Frequency")) ? null : reader.GetString(reader.GetOrdinal("Frequency")),
                            Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName"))
                        };
                    }
                }
            }
            return null;
        }

        // 辅助方法：更新处方明细
        private async Task<bool> UpdatePrescriptionDetailAsync(EditPrescriptionDetailViewModel model)
        {
            string sql = @"
        UPDATE PrescriptionDetails
        SET 
            MedicationID = @MedicationID,
            DoseValue = @DoseValue,
            DoseUnit = @DoseUnit,
            Quantity = @Quantity,
            Frequency = @Frequency,
            Duration = @Duration,
            Remarks = @Remarks
        WHERE DetailID = @DetailID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MedicationID", model.MedicationID);
                cmd.Parameters.AddWithValue("@DoseValue", model.DoseValue);
                cmd.Parameters.AddWithValue("@DoseUnit", model.DoseUnit ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                cmd.Parameters.AddWithValue("@Frequency", model.Frequency ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Duration", model.Duration);
                cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DetailID", model.DetailID);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }


}
