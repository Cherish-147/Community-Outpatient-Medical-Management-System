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
                // 休眠 0.2 秒（200 毫秒）
                //await Task.Delay(200);
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
                    await conn.OpenAsync();
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
        public async Task<IActionResult> Create(int? id)
        {
            var viewModel = new CreatePrescriptionViewModel();

            // 获取可开处方的预约列表（状态为“已叫号”或“已就诊”）
            viewModel.Appointments = await GetAppointmentSelectListAsync();
            if (id.HasValue) { viewModel.AppointmentID = (int)id; }
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
        public async Task<IActionResult> EditDetail(UpdatePrescriptionDetailModel model)
        {
            if (!ModelState.IsValid)
            {
                // 验证失败，需要重新加载完整视图模型并返回
                var viewModel = await GetEditPrescriptionDetailViewModelAsync(model.DetailID);
                if (viewModel == null) return NotFound();
                // 将用户已输入的值回填到视图模型（防止丢失）
                viewModel.MedicationID = model.MedicationID;
                viewModel.DoseValue = model.DoseValue;
                viewModel.DoseUnit = model.DoseUnit;
                viewModel.Quantity = model.Quantity;
                viewModel.Frequency = model.Frequency;
                viewModel.Duration = model.Duration;
                viewModel.Remarks = model.Remarks;

                // 重新加载药品下拉列表
                viewModel.Medications = await GetMedicationSelectListAsync();

                return View(viewModel);
            }
            try
            {
                bool success = await UpdatePrescriptionDetailAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "处方明细更新成功！";
                    //return RedirectToAction("Details", new { id = model.DetailID });
                    int prescriptionId = await GetPrescriptionIdByDetailIdAsync(model.DetailID);
                    return RedirectToAction("Details", new { id = prescriptionId });
                }
                else
                {
                    ModelState.AddModelError("", "未找到对应记录，更新失败");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失败：{ex.Message}");
            }

            // 如果更新失败，重新加载完整视图模型并返回
            var failedViewModel = await GetEditPrescriptionDetailViewModelAsync(model.DetailID);
            if (failedViewModel != null)
            {
                failedViewModel.MedicationID = model.MedicationID;
                failedViewModel.DoseValue = model.DoseValue;
                failedViewModel.DoseUnit = model.DoseUnit;
                failedViewModel.Quantity = model.Quantity;
                failedViewModel.Frequency = model.Frequency;
                failedViewModel.Duration = model.Duration;
                failedViewModel.Remarks = model.Remarks;
                failedViewModel.Medications = await GetMedicationSelectListAsync();
                return View(failedViewModel);

            }
            return NotFound();
        }
        private async Task<bool> UpdatePrescriptionDetailAsync(UpdatePrescriptionDetailModel model)//方法，更新处方明细
        {
            string sql = @"
        UPDATE PrescriptionDetails
        SET MedicationID = @MedicationID,
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

        // GET: /Prescription/Details/5
        public async Task<IActionResult> Details(int id)  // id 为 PrescriptionID
        {
            var viewModel = await GetPrescriptionDetailViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        private async Task<PrescriptionDetailViewModel> GetPrescriptionDetailViewModelAsync(int detailID)//(int prescriptionId)
        {
            string sql = @"
        SELECT 
            pr.PrescriptionID,
            pr.AppointmentID,
            pr.CreatedAt,
            p.Name AS PatientName,
            pd.DetailID,
            m.Name AS MedicationName,
            pd.DoseValue,
            pd.DoseUnit,
            pd.Quantity,
            pd.Frequency,
            pd.Duration,
            pd.Remarks
        FROM Prescriptions pr
        INNER JOIN Appointments a ON pr.AppointmentID = a.AppointmentID
        INNER JOIN Patients p ON a.PatientID = p.PatientID
        LEFT JOIN PrescriptionDetails pd ON pr.PrescriptionID = pd.PrescriptionID
        LEFT JOIN Medications m ON pd.MedicationID = m.MedicationID
        --WHERE pr.PrescriptionID = @PrescriptionID
        WHERE pr.PrescriptionID = @PrescriptionID
        ORDER BY pd.DetailID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                //cmd.Parameters.AddWithValue("@PrescriptionID", prescriptionId);
                cmd.Parameters.AddWithValue("@DetailID", detailID);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    PrescriptionDetailViewModel result = null;
                    while (await reader.ReadAsync())
                    {
                        if (result == null)
                        {
                            result = new PrescriptionDetailViewModel
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                Details = new List<PrescriptionDetailItem>()
                            };
                        }

                        // 如果有明细，添加明细
                        if (!reader.IsDBNull(reader.GetOrdinal("DetailID")))
                        {
                            result.Details.Add(new PrescriptionDetailItem
                            {
                                DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                                MedicationName = reader.GetString(reader.GetOrdinal("MedicationName")),
                                DoseValue = reader.GetDecimal(reader.GetOrdinal("DoseValue")),
                                DoseUnit = reader.IsDBNull(reader.GetOrdinal("DoseUnit")) ? null : reader.GetString(reader.GetOrdinal("DoseUnit")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Frequency = reader.IsDBNull(reader.GetOrdinal("Frequency")) ? null : reader.GetString(reader.GetOrdinal("Frequency")),
                                Duration = reader.IsDBNull(reader.GetOrdinal("Duration")) ? 0 : reader.GetInt32(reader.GetOrdinal("Duration")),
                                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks"))
                            });
                        }
                    }
                    return result;
                }
            }
        }

        private async Task<int> GetPrescriptionIdByDetailIdAsync(int detailId)
        {
            string sql = "SELECT PrescriptionID FROM PrescriptionDetails WHERE DetailID = @DetailID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@DetailID", detailId);
                await conn.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDetail(int detailId)//控制器--删除处方和明细
        {
            // 先获取该明细所属的处方ID，以便删除后跳转回详情页
            int prescriptionId = await GetPrescriptionIdByDetailIdAsync(detailId);
            if (prescriptionId == 0)
            {
                TempData["ErrorMessage"] = "未找到该明细记录";
                return RedirectToAction("Index");
            }

            try
            {
                string sql = "DELETE FROM PrescriptionDetails WHERE DetailID = @DetailID";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DetailID", detailId);
                    await conn.OpenAsync();
                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows > 0)
                    {
                        TempData["SuccessMessage"] = "药品明细删除成功！";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "未找到该明细记录";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除失败：{ex.Message}";
            }

            return RedirectToAction("Details", new { id = prescriptionId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePrescription(int id) // id 为 PrescriptionID
        {
            try
            {
                // 先删除所有关联明细（如果数据库未设置级联删除）
                string deleteDetailsSql = "DELETE FROM PrescriptionDetails WHERE PrescriptionID = @PrescriptionID";
                // 再删除主表
                string deletePrescriptionSql = "DELETE FROM Prescriptions WHERE PrescriptionID = @PrescriptionID";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 删除明细
                            using (SqlCommand cmdDetails = new SqlCommand(deleteDetailsSql, conn, transaction))
                            {
                                cmdDetails.Parameters.AddWithValue("@PrescriptionID", id);
                                int rows = await cmdDetails.ExecuteNonQueryAsync();
                                if (rows == 0)
                                {
                                    throw new Exception("未找到对应处方明细");
                                }
                            }

                            // 删除主表
                            using (SqlCommand cmdPres = new SqlCommand(deletePrescriptionSql, conn, transaction))
                            {
                                cmdPres.Parameters.AddWithValue("@PrescriptionID", id);
                                int rows = await cmdPres.ExecuteNonQueryAsync();
                                if (rows == 0)
                                {
                                    throw new Exception("未找到对应处方");
                                }
                            }

                            transaction.Commit();
                            TempData["SuccessMessage"] = "处方删除成功！";
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除失败：{ex.Message}";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> CreateDetail(int prescriptionId)
        {
            // 验证处方是否存在
            var prescriptionExists = await CheckPrescriptionExistsAsync(prescriptionId);
            if (!prescriptionExists)
            {
                return NotFound();
            }

            var model = new CreatePrescriptionDetailModel
            {
                PrescriptionID = prescriptionId,
                Medications = await GetMedicationSelectListAsync()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDetail(CreatePrescriptionDetailModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Medications = await GetMedicationSelectListAsync();
                return View(model);
            }

            try
            {
                string insertSql = @"
            INSERT INTO PrescriptionDetails 
            (PrescriptionID, MedicationID, DoseValue, DoseUnit, Quantity, Frequency, Duration, Remarks)
            VALUES (@PrescriptionID, @MedicationID, @DoseValue, @DoseUnit, @Quantity, @Frequency, @Duration, @Remarks)";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@PrescriptionID", model.PrescriptionID);
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

                TempData["SuccessMessage"] = "药品明细添加成功！";
                return RedirectToAction("Details", new { id = model.PrescriptionID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"添加失败：{ex.Message}");
                model.Medications = await GetMedicationSelectListAsync();
                return View(model);
            }

        }
        private async Task<bool> CheckPrescriptionExistsAsync(int prescriptionId)
        {
            string sql = "SELECT COUNT(1) FROM Prescriptions WHERE PrescriptionID = @PrescriptionID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PrescriptionID", prescriptionId);
                await conn.OpenAsync();
                return (int)await cmd.ExecuteScalarAsync() > 0;
            }
        }

    }


}
