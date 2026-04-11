using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False";
        #region 患者管理
        public async Task<IActionResult> GetAllPatientsIndex()//控制器，患者列表首页(管理端)
        {
           var patientsList = await GetAllPatientsInfoAsync();
            return View(patientsList);
        }
        public virtual async Task<List<Patient>> GetAllPatientsInfoAsync()//方法--获取所有患者信息(管理端)
        {
            var patientsList = new List<Patient>();
            string sql = @"SELECT [PatientID]
                                         ,[UserId]
                                         ,[Name]
                                         ,[Birthday]
                                         ,[Gender]
                                         ,[IDCard]
                                         ,[Phone]
                                         ,[InsuranceNo]
                                         ,[CreatedAt]
                                         ,[UpdatedAt]
                                         ,[IsMarried]
                                         ,[Nation]
                                         ,[WorkUnit]
                                         ,[Occupation]
                                         ,[Address]
                                         ,[PastMedicalHistory]
                                         ,[DrugAllergyHistory]
                                         ,[GuardianName]
                                         ,[GuardianRelationship]
                                         ,[GuardianAddress]
                                         ,[GuardianPhone]
                                         ,[Remark]
                                     FROM [Patients]";
            try
            {

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                   
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }
                    using (SqlCommand cmd = new SqlCommand(sql, conn))

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var patient = new Patient
                            {
                                PatientID = reader.IsDBNull(reader.GetOrdinal("PatientID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PatientID")),
                                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                                Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader.GetString(reader.GetOrdinal("Gender")),
                                IDCard = reader.IsDBNull(reader.GetOrdinal("IDCard")) ? null : reader.GetString(reader.GetOrdinal("IDCard")),
                                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                                InsuranceNo = reader.IsDBNull(reader.GetOrdinal("InsuranceNo")) ? null : reader.GetString(reader.GetOrdinal("InsuranceNo")),
                                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                                Nation = reader.IsDBNull(reader.GetOrdinal("Nation")) ? null : reader.GetString(reader.GetOrdinal("Nation")),
                                WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit")) ? null : reader.GetString(reader.GetOrdinal("WorkUnit")),
                                Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? null : reader.GetString(reader.GetOrdinal("Occupation")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                                DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory")) ? null : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                                GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName")) ? null : reader.GetString(reader.GetOrdinal("GuardianName")),
                                GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship")) ? null : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                                GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress")) ? null : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                                GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone")) ? null : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                                Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? null : reader.GetString(reader.GetOrdinal("Remark"))
                            };
                            patientsList.Add(patient);
                        }
                    } 

                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取所有患者失败：" + ex.Message);
                return null;
            }
            return patientsList;
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatientsIndex(string name, int? patientId)
        {
            List<Patient> patientsList;

            // 如果有搜索条件，调用搜索方法；否则获取全部
            if (!string.IsNullOrWhiteSpace(name) || patientId.HasValue)
            {
                patientsList = await SearchPatientsAsync(name, patientId);
                // 将搜索条件传递到视图，用于回显
                ViewBag.Name = name;          // 回显搜索词
                ViewBag.PatientId = patientId; // 回显ID
            }
            else
            {
                patientsList = await GetAllPatientsInfoAsync();
            }

            return View(patientsList);
        }

        public async Task<List<Patient>> SearchPatientsAsync(string name, int? patientId)
        {
            var patientsList = new List<Patient>();

            // 构建动态SQL，使用参数化查询防止注入
            string sql = @"SELECT [PatientID], [UserId], [Name], [Birthday], [Gender], [IDCard], 
                          [Phone], [InsuranceNo], [CreatedAt], [UpdatedAt], [IsMarried], 
                          [Nation], [WorkUnit], [Occupation], [Address], [PastMedicalHistory], 
                          [DrugAllergyHistory], [GuardianName], [GuardianRelationship], 
                          [GuardianAddress], [GuardianPhone], [Remark]
                   FROM [Patients]
                   WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += " AND [Name] LIKE @Name";
                parameters.Add(new SqlParameter("@Name", $"%{name}%"));
            }

            if (patientId.HasValue)
            {
                sql += " AND [PatientID] = @PatientId";
                parameters.Add(new SqlParameter("@PatientId", patientId.Value));
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // 使用之前提供的安全读取方法（已包含空值处理）
                        patientsList.Add(MapToPatient(reader));
                    }
                }
            }

            return patientsList;
        }

        // 将读取逻辑提取为辅助方法（可选）
        private Patient MapToPatient(SqlDataReader reader)
        {
            return new Patient
            {
                PatientID = reader.IsDBNull(reader.GetOrdinal("PatientID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PatientID")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader.GetString(reader.GetOrdinal("Gender")),
                IDCard = reader.IsDBNull(reader.GetOrdinal("IDCard")) ? null : reader.GetString(reader.GetOrdinal("IDCard")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                InsuranceNo = reader.IsDBNull(reader.GetOrdinal("InsuranceNo")) ? null : reader.GetString(reader.GetOrdinal("InsuranceNo")),
                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                Nation = reader.IsDBNull(reader.GetOrdinal("Nation")) ? null : reader.GetString(reader.GetOrdinal("Nation")),
                WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit")) ? null : reader.GetString(reader.GetOrdinal("WorkUnit")),
                Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? null : reader.GetString(reader.GetOrdinal("Occupation")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory")) ? null : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName")) ? null : reader.GetString(reader.GetOrdinal("GuardianName")),
                GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship")) ? null : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress")) ? null : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone")) ? null : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? null : reader.GetString(reader.GetOrdinal("Remark"))
            };
        }


        // GET: /Patient/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var users = await GetUnlinkedUserIds();
            var userItems = users.Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = $"{u.UserId} - {u.UserName}"
            }).ToList();
            ViewBag.Users = userItems;
            return View();
        }

        // POST: /Patient/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Patient model)
        {
            // 移除可能的 ModelState 错误（例如 CreatedAt 自动生成，无需验证）
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 设置创建时间
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = null;  // 新记录没有更新时间

            // 调用插入方法
            bool success = await InsertPatientAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "患者信息添加成功！";
                return RedirectToAction(nameof(GetAllPatientsIndex)); // 跳转到患者列表
            }
            else
            {
                TempData["ErrorMessage"] = "添加失败，请稍后重试。";
                return View(model);
            }
        }
        public async Task<bool> InsertPatientAsync(Patient patient)
        {
            string insertSql = @"
        INSERT INTO Patients 
        (UserId, Name, Birthday, Gender, IDCard, Phone, InsuranceNo, 
         CreatedAt, UpdatedAt, IsMarried, Nation, WorkUnit, Occupation, 
         Address, PastMedicalHistory, DrugAllergyHistory, GuardianName, 
         GuardianRelationship, GuardianAddress, GuardianPhone, Remark)
        OUTPUT INSERTED.PatientID
        VALUES 
        (@UserId, @Name, @Birthday, @Gender, @IDCard, @Phone, @InsuranceNo,
         @CreatedAt, @UpdatedAt, @IsMarried, @Nation, @WorkUnit, @Occupation,
         @Address, @PastMedicalHistory, @DrugAllergyHistory, @GuardianName,
         @GuardianRelationship, @GuardianAddress, @GuardianPhone, @Remark)";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", patient.UserId == 0 ? (object)DBNull.Value : patient.UserId);
                cmd.Parameters.AddWithValue("@Name", patient.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Birthday", patient.Birthday ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", patient.Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IDCard", patient.IDCard ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", patient.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InsuranceNo", patient.InsuranceNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", patient.CreatedAt);
                cmd.Parameters.AddWithValue("@UpdatedAt", patient.UpdatedAt ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsMarried", patient.IsMarried ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Nation", patient.Nation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkUnit", patient.WorkUnit ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Occupation", patient.Occupation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", patient.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PastMedicalHistory", patient.PastMedicalHistory ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DrugAllergyHistory", patient.DrugAllergyHistory ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianName", patient.GuardianName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianRelationship", patient.GuardianRelationship ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianAddress", patient.GuardianAddress ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianPhone", patient.GuardianPhone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remark", patient.Remark ?? (object)DBNull.Value);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<List<BindUsers>> GetUnlinkedUserIds()//方法--获取所有用户信息（用于患者注册时选择关联用户）
        {
            var users = new List<BindUsers>();
            string sql = @"
select u.UserId,u.UserName from Users u 
left join
Patients p on p.UserId =u.UserId
where p.UserId is null 
and u.Role='Patient'
/*
select u.UserId,u.UserName from Users u where u.UserId 
not in (select p.UserId from Patients p) and role='Patient'
*/
";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new BindUsers
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            UserName = reader.GetString(reader.GetOrdinal("UserName"))
                        });
                    }
                }
            }
            return users;
        }

        //患者详细信息
        [HttpGet]
        public async Task<IActionResult> DetailpatientByPatientId(int patientId)
        {
            ViewBag.PatientId1 = HttpContext.Session.GetInt32("patientId");

            var result = await  DetailpatientByPatientIdInfo(patientId);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(GetAllPatientsIndex));
            }
            return View(result);

        }

        public virtual async Task<DetailPatientModel> DetailpatientByPatientIdInfo(int patientId)
        {
            var patient = new DetailPatientModel();
            string sql = @"
SELECT  [PatientID]
      ,[UserId]
      ,[Name]
      ,[Birthday]
      ,[Gender]
      ,[IDCard]
      ,[Phone]
      ,[InsuranceNo]
      ,[CreatedAt]
      ,[UpdatedAt]
      ,[IsMarried]
      ,[Nation]
      ,[WorkUnit]
      ,[Occupation]
      ,[Address]
      ,[PastMedicalHistory]
      ,[DrugAllergyHistory]
      ,[GuardianName]
      ,[GuardianRelationship]
      ,[GuardianAddress]
      ,[GuardianPhone]
      ,[Remark]
  FROM [Community-Outpatient-Medical-Management-System].[dbo].[Patients]
  where PatientID=@PatientID
";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PatientID", patientId);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new DetailPatientModel
                            {
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                                IDCard = reader.GetString(reader.GetOrdinal("IDCard")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                InsuranceNo = reader.GetString(reader.GetOrdinal("InsuranceNo")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                                Nation = reader.IsDBNull(reader.GetOrdinal("Nation")) ? null : reader.GetString(reader.GetOrdinal("Nation")),
                                WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit")) ? null : reader.GetString(reader.GetOrdinal("WorkUnit")),
                                Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? null : reader.GetString(reader.GetOrdinal("Occupation")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                                DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory")) ? null : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                                GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName")) ? null : reader.GetString(reader.GetOrdinal("GuardianName")),
                                GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship")) ? null : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                                GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress")) ? null : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                                GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone")) ? null : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                                Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? null : reader.GetString(reader.GetOrdinal("Remark")),
                                IsSuccess = true,
                                Message = null
                            };
                        }
                        else
                        {
                            return new DetailPatientModel
                            {
                                IsSuccess = false,
                                Message = $"未找到ID为 {patientId} 的患者记录。"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new DetailPatientModel
                {
                    IsSuccess = false,
                    Message = $"查询失败：{ex.Message}"
                };
            }
        
            return patient;
        }

        // 患者信息编辑Start
        // 获取患者信息用于编辑
        public virtual async Task<EditPatientModel> GetPatientForEditAsync(int patientId)
        {
            string sql = @"
        SELECT PatientID, UserId, Name, Birthday, Gender, IDCard, Phone, 
               InsuranceNo, CreatedAt, UpdatedAt, IsMarried, Nation, WorkUnit, 
               Occupation, Address, PastMedicalHistory, DrugAllergyHistory, 
               GuardianName, GuardianRelationship, GuardianAddress, GuardianPhone, Remark
        FROM Patients 
        WHERE PatientID = @PatientID";


            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PatientID", patientId);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new EditPatientModel
                            {
                                IsSuccess = true,
                                Message = null,
                                // PatientID 不需要在模型中展示，但可以内部使用，不过模型已注释，可忽略
                                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                                IDCard = reader.GetString(reader.GetOrdinal("IDCard")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                InsuranceNo = reader.GetString(reader.GetOrdinal("InsuranceNo")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                                Nation = reader.IsDBNull(reader.GetOrdinal("Nation")) ? null : reader.GetString(reader.GetOrdinal("Nation")),
                                WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit")) ? null : reader.GetString(reader.GetOrdinal("WorkUnit")),
                                Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? null : reader.GetString(reader.GetOrdinal("Occupation")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                                DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory")) ? null : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                                GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName")) ? null : reader.GetString(reader.GetOrdinal("GuardianName")),
                                GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship")) ? null : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                                GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress")) ? null : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                                GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone")) ? null : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                                Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? null : reader.GetString(reader.GetOrdinal("Remark"))
                            };
                        }
                        else
                        {
                            return new EditPatientModel
                            {
                                IsSuccess = false,
                                Message = $"未找到ID为 {patientId} 的患者。"
                            };
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return new EditPatientModel
                {
                    IsSuccess = false,
                    Message = $"查询失败：{ex.Message}"
                };
            }
        }

        // 更新患者信息
        public virtual async Task<bool> UpdatePatientAsync(EditPatientModel model, int patientId)
        {
            string updateSql = @"
        UPDATE Patients 
        SET UserId = @UserId,
            Name = @Name,
            Birthday = @Birthday,
            Gender = @Gender,
            IDCard = @IDCard,
            Phone = @Phone,
            InsuranceNo = @InsuranceNo,
            UpdatedAt = @UpdatedAt,
            IsMarried = @IsMarried,
            Nation = @Nation,
            WorkUnit = @WorkUnit,
            Occupation = @Occupation,
            Address = @Address,
            PastMedicalHistory = @PastMedicalHistory,
            DrugAllergyHistory = @DrugAllergyHistory,
            GuardianName = @GuardianName,
            GuardianRelationship = @GuardianRelationship,
            GuardianAddress = @GuardianAddress,
            GuardianPhone = @GuardianPhone,
            Remark = @Remark
        WHERE PatientID = @PatientID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", model.UserId == 0 ? (object)DBNull.Value : model.UserId);
                cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Birthday", model.Birthday ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", model.Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IDCard", model.IDCard ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InsuranceNo", model.InsuranceNo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@IsMarried", model.IsMarried ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Nation", model.Nation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkUnit", model.WorkUnit ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Occupation", model.Occupation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", model.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PastMedicalHistory", model.PastMedicalHistory ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DrugAllergyHistory", model.DrugAllergyHistory ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianName", model.GuardianName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianRelationship", model.GuardianRelationship ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianAddress", model.GuardianAddress ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuardianPhone", model.GuardianPhone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remark", model.Remark ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PatientID", patientId);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
        public async Task<List<BindUsers>> GetAllUsersAsync()//方法--获取所有用户信息（用于患者注册时选择关联用户）
        {
            var users = new List<BindUsers>();
            string sql = "SELECT UserId, UserName FROM Users where Role ='Patient'";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new BindUsers
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            UserName = reader.GetString(reader.GetOrdinal("UserName"))
                        });
                    }
                }
            }
            return users;
        }
        //控制器
        [HttpGet]
        public async Task<IActionResult> EditpatientByPatientId(int id)
        {
            var model = await GetPatientForEditAsync(id);
            if (!model.IsSuccess)
            {
                TempData["ErrorMessage"] = model.Message;
                return RedirectToAction(nameof(GetAllPatientsIndex));
            }
            var users = await GetAllUsersAsync();
            ViewBag.Users = new SelectList(users, "UserId", "UserName", model.UserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditpatientByPatientId(int id, EditPatientModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await GetAllUsersAsync();
                ViewBag.Users = new SelectList(users, "UserId", "UserName", model.UserId);
                return View(model);
            }

            bool success = await UpdatePatientAsync(model, id);
            if (success)
            {
                TempData["SuccessMessage"] = "患者信息更新成功！";
                return RedirectToAction(nameof(DetailpatientByPatientId), new { patientId = id });
            }
            else
            {
                TempData["ErrorMessage"] = "更新失败，请稍后重试。";
                var users = await GetAllUsersAsync();
                ViewBag.Users = new SelectList(users, "UserId", "UserName", model.UserId);
                return View(model);
            }
        }

        //患者信息编辑END
        #endregion
        // 患者首页
        // 1. 挂号首页（选择排班+患者信息）- 优化剩余号源计算
        public IActionResult Index()
        {
            // 验证登录状态（患者/管理员均可访问）
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Home");
            }

            // 核心新增：患者角色校验是否有患者信息，无则跳转到注册
            if (HttpContext.Session.GetString("Role") == "Patient")
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                // 查询该用户是否有对应的患者信息
                DataTable dtPatient = GetPatientByUserId(userId);
                if (dtPatient == null || dtPatient.Rows.Count == 0 || Convert.ToInt32(dtPatient.Rows[0]["PatientID"]) <= 0)
                {
                    // 跳转到患者注册页面，并携带返回地址（注册成功后回到挂号页）
                    TempData["Tips"] = "你还未完善患者信息，请先完成注册！";
                    return RedirectToAction("Register", "Patient", new { returnUrl = "/Appointment/Index" });
                }
            }
            // 获取可预约的排班（未约满、未过期）
            DataTable dtAvailableSchedules = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 优化SQL：仅统计「已预约」状态的挂号，排除「已取消」的，确保剩余号源准确
                    string sql = @"
                SELECT s.ScheduleID, d.DoctorName, dp.DeptName, s.Date, s.TimeSlot, 
                       s.MaxAppointments, 
                       -- 仅统计已预约的挂号数（排除已取消）
                       (SELECT COUNT(1) FROM dbo.Appointments a WHERE a.ScheduleID = s.ScheduleID AND a.[Status] = N'已预约') AS UsedAppointments,
                       -- 剩余号源 = 总号源 - 已预约数
                       (s.MaxAppointments - (SELECT COUNT(1) FROM dbo.Appointments a WHERE a.ScheduleID = s.ScheduleID AND a.[Status] = N'已预约')) AS Remaining
                FROM dbo.Schedules s
                LEFT JOIN dbo.Doctors d ON s.DoctorID = d.DoctorID
                LEFT JOIN dbo.Departments dp ON d.DeptID = dp.DeptID
                WHERE s.Date >= CONVERT(DATE, GETDATE()) AND d.IsActive = 1
                -- 仅显示剩余号源>0的排班
                HAVING (s.MaxAppointments - (SELECT COUNT(1) FROM dbo.Appointments a WHERE a.ScheduleID = s.ScheduleID AND a.[Status] = N'已预约')) > 0
                ORDER BY s.Date, s.TimeSlot";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dtAvailableSchedules);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "加载可预约排班失败：" + ex.Message;
            }

            // 获取患者列表（仅管理员可见，患者仅能看到自己）
            if (HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "Staff")
            {
                ViewBag.Patients = GetAllPatients();
            }
            else if (HttpContext.Session.GetString("Role") == "Patient")
            {
                // 患者仅能看到自己的信息
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                ViewBag.Patients = GetPatientByUserId(userId);
            }

            return View(dtAvailableSchedules);
        }

        // 辅助方法：获取所有患者（管理员/医护人员用）
        private DataTable GetAllPatients()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 查询所有患者信息（按姓名排序）
                    string sql = "SELECT PatientID, Name FROM dbo.Patients ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                // 记录异常（可选），保证方法不会抛出异常导致程序崩溃
                Console.WriteLine("获取所有患者失败：" + ex.Message);
            }
            return dt;
        }

        // 辅助方法：根据用户ID获取患者（患者角色用）
        private DataTable GetPatientByUserId(int userId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 仅查询当前用户关联的患者信息
                    string sql = "SELECT PatientID, Name FROM dbo.Patients WHERE UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId); // 参数化查询防注入
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取当前用户患者信息失败：" + ex.Message);
            }
            return dt;
        }
        public IActionResult PatientIndex()
        {
            // 验证患者是否登录
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                HttpContext.Session.GetString("Role") != "Patient")
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        } // 预约挂号
        public IActionResult RegisterAppointment()
        {
            // 登录验证
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                HttpContext.Session.GetString("Role") != "Patient")
            {
                return RedirectToAction("Login", "Home");
            }
            ViewData["Title"] = "预约挂号 - 社区门诊患者中心";
            return View();
        }

        // 个人信息管理
        public IActionResult EditProfile()
        {
            // 登录验证
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                HttpContext.Session.GetString("Role") != "Patient")
            {
                return RedirectToAction("Login", "Home");
            }
            ViewData["Title"] = "个人信息管理 - 社区门诊患者中心";
            return View();
        }

        // 缴费记录
        public async Task<IActionResult> MyPayments()
        {
            // 登录验证
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                HttpContext.Session.GetString("Role") != "Patient")
            {
                return RedirectToAction("Login", "Home");
            }
            
            ViewData["Title"] = "缴费记录 - 社区门诊患者中心";
            // 获取当前登录患者的 PatientID（假设 Session 中存有 PatientID 或通过 UserID 查询）
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            // 根据 UserId 获取 PatientID（可能需要额外方法，假设你有 GetPatientByUserId 方法）
            int patientId = await GetPatientIdByUserIdAsync(userId); // 自行实现
            if (patientId == 0)
            {
                // 如果没有患者信息，跳转到注册页面
                TempData["Error"] = "请先完善患者信息。";
                return RedirectToAction("Register", "Patient");
            }

            var payments = await GetMyPayment(patientId);
            return View(payments);
           
        }
        public virtual async Task<List<GetMyPaymentModel>> GetMyPayment(int patientID)//获取当前患者的缴费记录方法
        {
            var paymentList = new List<GetMyPaymentModel>();
            string sql = @"
                         SELECT pay.PaymentID, pay.AppointmentID,
                                p.PatientID, p.Name AS PatientName,
                                pay.Amount, pay.Method, pay.Status, pay.PaidAt
                         FROM Payments pay
                         INNER JOIN Appointments a ON pay.AppointmentID = a.AppointmentID
                         INNER JOIN Patients p ON a.PatientID = p.PatientID
                         WHERE p.PatientID = @PatientID
                         ORDER BY pay.PaymentID DESC
                        ";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientID);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var model = new GetMyPaymentModel
                        {
                            PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            Method = reader.IsDBNull(reader.GetOrdinal("Method")) ? null : reader.GetString(reader.GetOrdinal("Method")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                            PaidAt = reader.GetDateTime(reader.GetOrdinal("PaidAt"))
                        };
                        paymentList.Add(model);
                    }
                }
            }
            return paymentList;
        }
        public async Task<int> GetPatientIdByUserIdAsync(int userId)
        {
            string sql = "SELECT PatientID FROM Patients WHERE UserId = @UserId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                await conn.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        // 根据 PaymentID 获取支付记录（可复用已有方法）
        private async Task<GetMyPaymentModel> GetPaymentByIdAsync(int paymentId)
        {
            string sql = @"SELECT pay.PaymentID, pay.AppointmentID, p.PatientID, p.Name AS PatientName,
                          pay.Amount, pay.Method, pay.Status, pay.PaidAt
                   FROM Payments pay
                   INNER JOIN Appointments a ON pay.AppointmentID = a.AppointmentID
                   INNER JOIN Patients p ON a.PatientID = p.PatientID
                   WHERE pay.PaymentID = @PaymentID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new GetMyPaymentModel
                        {
                            PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            Method = reader.IsDBNull(reader.GetOrdinal("Method")) ? null : reader.GetString(reader.GetOrdinal("Method")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                            PaidAt = reader.GetDateTime(reader.GetOrdinal("PaidAt"))
                        };
                    }
                }
            }
            return null;
        }


        //患者信息首页
        [HttpGet]
        public IActionResult GetPatientInfo()
        {
            // 登录验证 + 角色验证（仅患者可访问）
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) || HttpContext.Session.GetString("Role") != "Patient")
            {
                TempData["Error"] = "仅患者可访问个人信息管理！";
                return RedirectToAction("Login", "Home");
            }
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            ViewBag.UserId = userId;
            DataTable dtPatient = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT PatientID, UserId, [Name], Birthday, Gender, IsMarried, Nation, IDCard, Phone,
                               InsuranceNo, WorkUnit, Occupation, [Address], PastMedicalHistory, DrugAllergyHistory,
                               GuardianName, GuardianAddress, GuardianPhone, GuardianRelationship, Remark,
                               CreatedAt, UpdatedAt
                        FROM dbo.Patients 
                        WHERE UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dtPatient);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "加载患者信息失败：" + ex.Message;
            }

            return View(dtPatient);

        }

        #region 编辑患者信息
        // 4. 编辑患者信息（页面）
        public IActionResult Edit(int id)
        {
            // 登录验证 + 角色验证
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) || HttpContext.Session.GetString("Role") != "Patient")
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            DataRow patientRow = GetPatientInfoById(id, userId);

            if (patientRow == null)
            {
                TempData["Error"] = "患者信息不存在或无权限访问！";
                return RedirectToAction("Index");
            }

            ViewBag.PatientData = patientRow;
            return View();
        }

        // 5. 编辑患者信息（提交）
        [HttpPost]
        public IActionResult Edit(
            int PatientID, string Name, DateTime? Birthday, string Gender, bool? IsMarried, string Nation,
            string IDCard, string Phone, string InsuranceNo, string WorkUnit, string Occupation,
            string Address, string PastMedicalHistory, string DrugAllergyHistory,
            string GuardianName, string GuardianAddress, string GuardianPhone, string GuardianRelationship,
            string Remark)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                // 验证权限（确保是当前用户的信息）
                if (!IsPatientInfoBelongToUser(PatientID, userId))
                {
                    TempData["Error"] = "无权限修改该患者信息！";
                    return RedirectToAction("Index");
                }

                // 基础验证
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(IDCard) || string.IsNullOrEmpty(Phone))
                {
                    ViewBag.Error = "姓名、身份证号、联系电话为必填项！";
                    ViewBag.FormData = Request.Form;
                    return View();
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 检查身份证号是否重复（排除自己）
                    if (IsIDCardExists(IDCard, PatientID))
                    {
                        ViewBag.Error = "该身份证号已绑定其他患者！";
                        ViewBag.FormData = Request.Form;
                        return View();
                    }

                    string sql = @"
                        UPDATE dbo.Patients SET
                            [Name] = @Name,
                            Birthday = @Birthday,
                            Gender = @Gender,
                            IsMarried = @IsMarried,
                            Nation = @Nation,
                            IDCard = @IDCard,
                            Phone = @Phone,
                            InsuranceNo = @InsuranceNo,
                            WorkUnit = @WorkUnit,
                            Occupation = @Occupation,
                            [Address] = @Address,
                            PastMedicalHistory = @PastMedicalHistory,
                            DrugAllergyHistory = @DrugAllergyHistory,
                            GuardianName = @GuardianName,
                            GuardianAddress = @GuardianAddress,
                            GuardianPhone = @GuardianPhone,
                            GuardianRelationship = @GuardianRelationship,
                            Remark = @Remark,
                            UpdatedAt = GETDATE()
                        WHERE PatientID = @PatientID AND UserId = @UserId";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    // 绑定参数
                    cmd.Parameters.AddWithValue("@PatientID", PatientID);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Birthday", Birthday ?? null);
                    cmd.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(Gender) ? DBNull.Value : (object)Gender);
                    cmd.Parameters.AddWithValue("@IsMarried", IsMarried ?? null);
                    cmd.Parameters.AddWithValue("@Nation", string.IsNullOrEmpty(Nation) ? DBNull.Value : (object)Nation);
                    cmd.Parameters.AddWithValue("@IDCard", IDCard);
                    cmd.Parameters.AddWithValue("@Phone", Phone);
                    cmd.Parameters.AddWithValue("@InsuranceNo", string.IsNullOrEmpty(InsuranceNo) ? DBNull.Value : (object)InsuranceNo);
                    cmd.Parameters.AddWithValue("@WorkUnit", string.IsNullOrEmpty(WorkUnit) ? DBNull.Value : (object)WorkUnit);
                    cmd.Parameters.AddWithValue("@Occupation", string.IsNullOrEmpty(Occupation) ? DBNull.Value : (object)Occupation);
                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(Address) ? DBNull.Value : (object)Address);
                    cmd.Parameters.AddWithValue("@PastMedicalHistory", string.IsNullOrEmpty(PastMedicalHistory) ? DBNull.Value : (object)PastMedicalHistory);
                    cmd.Parameters.AddWithValue("@DrugAllergyHistory", string.IsNullOrEmpty(DrugAllergyHistory) ? DBNull.Value : (object)DrugAllergyHistory);
                    cmd.Parameters.AddWithValue("@GuardianName", string.IsNullOrEmpty(GuardianName) ? DBNull.Value : (object)GuardianName);
                    cmd.Parameters.AddWithValue("@GuardianAddress", string.IsNullOrEmpty(GuardianAddress) ? DBNull.Value : (object)GuardianAddress);
                    cmd.Parameters.AddWithValue("@GuardianPhone", string.IsNullOrEmpty(GuardianPhone) ? DBNull.Value : (object)GuardianPhone);
                    cmd.Parameters.AddWithValue("@GuardianRelationship", string.IsNullOrEmpty(GuardianRelationship) ? DBNull.Value : (object)GuardianRelationship);
                    cmd.Parameters.AddWithValue("@Remark", string.IsNullOrEmpty(Remark) ? DBNull.Value : (object)Remark);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "患者信息修改成功！";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Error = "修改失败，请重试！";
                        ViewBag.FormData = Request.Form;
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "修改患者信息出错：" + ex.Message;
                ViewBag.FormData = Request.Form;
                return View();
            }
        }
        // 检查身份证号是否重复
        private bool IsIDCardExists(string idCard, int excludePatientId = 0)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM dbo.Patients WHERE IDCard = @IDCard";
                if (excludePatientId > 0)
                {
                    sql += " AND PatientID != @ExcludePatientId";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDCard", idCard);
                if (excludePatientId > 0)
                {
                    cmd.Parameters.AddWithValue("@ExcludePatientId", excludePatientId);
                }

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        // 检查患者信息是否属于当前用户
        private bool IsPatientInfoBelongToUser(int patientId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM dbo.Patients WHERE PatientID = @PatientID AND UserId = @UserId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patientId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        // 根据ID和用户ID获取患者信息
        private DataRow GetPatientInfoById(int patientId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT * FROM dbo.Patients 
                    WHERE PatientID = @PatientID AND UserId = @UserId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patientId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }
        #endregion
        //注册账号后,判断是否Patient存在 UserId ，如果不存在强制填写患者注册
        // 新增：患者注册页面展示（无需登录验证，允许未注册用户访问）
        public IActionResult Register()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }
        // 新增：患者注册表单提交处理
        [HttpPost]
        public IActionResult Register(PatientRegisterModel model)
        {
            try
            {
                // 1. 基础验证
                if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.IDCard) ||
                    string.IsNullOrEmpty(model.Phone) || string.IsNullOrEmpty(model.Gender) ||
                    model.Birthday == DateTime.MinValue)
                {
                    ViewBag.Error = "姓名、身份证号、联系电话、性别、出生日期为必填项！";
                    return View(model);
                }

                // 2. 检查身份证号是否已存在
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string checkSql = "SELECT COUNT(1) FROM dbo.Patients WHERE IDCard = @IDCard";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@IDCard", model.IDCard);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        ViewBag.Error = "该身份证号已注册过患者信息！";
                        return View(model);
                    }

                    // 3. 插入患者数据（关联当前登录用户的UserID，未登录则设为0）
                    string insertSql = @"
                        INSERT INTO dbo.Patients (
                            UserId, Name, Birthday, Gender, IsMarried, Nation, IDCard, Phone, 
                            InsuranceNo, WorkUnit, Occupation, [Address], PastMedicalHistory, 
                            DrugAllergyHistory, GuardianName, GuardianAddress, GuardianPhone, 
                            GuardianRelationship, Remark, CreatedAt, UpdatedAt
                        ) VALUES (
                            @UserId, @Name, @Birthday, @Gender, @IsMarried, @Nation, @IDCard, @Phone, 
                            @InsuranceNo, @WorkUnit, @Occupation, @Address, @PastMedicalHistory, 
                            @DrugAllergyHistory, @GuardianName, @GuardianAddress, @GuardianPhone, 
                            @GuardianRelationship, @Remark, GETDATE(), GETDATE()
                        )";

                    SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                    // 绑定参数（优先使用当前登录用户的UserID）
                    string userId = HttpContext.Session.GetString("UserID");
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@Name", model.Name);
                    insertCmd.Parameters.AddWithValue("@Birthday", model.Birthday);
                    insertCmd.Parameters.AddWithValue("@Gender", model.Gender);
                    insertCmd.Parameters.AddWithValue("@IsMarried", string.IsNullOrEmpty(model.IsMarried) ? DBNull.Value : (object)Convert.ToBoolean(model.IsMarried));
                    insertCmd.Parameters.AddWithValue("@Nation", string.IsNullOrEmpty(model.Nation) ? DBNull.Value : (object)model.Nation);
                    insertCmd.Parameters.AddWithValue("@IDCard", model.IDCard);
                    insertCmd.Parameters.AddWithValue("@Phone", model.Phone);
                    insertCmd.Parameters.AddWithValue("@InsuranceNo", string.IsNullOrEmpty(model.InsuranceNo) ? DBNull.Value : (object)model.InsuranceNo);
                    insertCmd.Parameters.AddWithValue("@WorkUnit", string.IsNullOrEmpty(model.WorkUnit) ? DBNull.Value : (object)model.WorkUnit);
                    insertCmd.Parameters.AddWithValue("@Occupation", string.IsNullOrEmpty(model.Occupation) ? DBNull.Value : (object)model.Occupation);
                    insertCmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(model.Address) ? DBNull.Value : (object)model.Address);
                    insertCmd.Parameters.AddWithValue("@PastMedicalHistory", string.IsNullOrEmpty(model.PastMedicalHistory) ? DBNull.Value : (object)model.PastMedicalHistory);
                    insertCmd.Parameters.AddWithValue("@DrugAllergyHistory", string.IsNullOrEmpty(model.DrugAllergyHistory) ? DBNull.Value : (object)model.DrugAllergyHistory);
                    insertCmd.Parameters.AddWithValue("@GuardianName", string.IsNullOrEmpty(model.GuardianName) ? DBNull.Value : (object)model.GuardianName);
                    insertCmd.Parameters.AddWithValue("@GuardianAddress", string.IsNullOrEmpty(model.GuardianAddress) ? DBNull.Value : (object)model.GuardianAddress);
                    insertCmd.Parameters.AddWithValue("@GuardianPhone", string.IsNullOrEmpty(model.GuardianPhone) ? DBNull.Value : (object)model.GuardianPhone);
                    insertCmd.Parameters.AddWithValue("@GuardianRelationship", string.IsNullOrEmpty(model.GuardianRelationship) ? DBNull.Value : (object)model.GuardianRelationship);
                    insertCmd.Parameters.AddWithValue("@Remark", string.IsNullOrEmpty(model.Remark) ? DBNull.Value : (object)model.Remark);

                    // 执行插入
                    int rows = insertCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        // 注册成功后，若用户已登录，更新Session的Role为Patient
                        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                        {
                            HttpContext.Session.SetString("Role", "Patient");
                        }
                        ViewBag.Success = "患者信息注册成功！";
                        // 清空表单数据
                        ModelState.Clear();
                        return View();
                    }
                    else
                    {
                        ViewBag.Error = "注册失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "注册出错：" + ex.Message;
            }

            // 保留用户输入的表单数据
            ViewBag.Name = model.Name;
            ViewBag.Gender = model.Gender;
            ViewBag.Birthday = model.Birthday.ToString("yyyy-MM-dd");
            ViewBag.IsMarried = model.IsMarried;
            ViewBag.Nation = model.Nation;
            ViewBag.IDCard = model.IDCard;
            ViewBag.Phone = model.Phone;
            ViewBag.InsuranceNo = model.InsuranceNo;
            ViewBag.WorkUnit = model.WorkUnit;
            ViewBag.Occupation = model.Occupation;
            ViewBag.Address = model.Address;
            ViewBag.PastMedicalHistory = model.PastMedicalHistory;
            ViewBag.DrugAllergyHistory = model.DrugAllergyHistory;
            ViewBag.GuardianName = model.GuardianName;
            ViewBag.GuardianPhone = model.GuardianPhone;
            ViewBag.GuardianRelationship = model.GuardianRelationship;
            ViewBag.GuardianAddress = model.GuardianAddress;
            ViewBag.Remark = model.Remark;

            return View(model);
        }

        // 患者注册模型类（放在PatientController同级）
        public class PatientRegisterModel
        {
            public string Name { get; set; }
            public string Gender { get; set; }
            public DateTime Birthday { get; set; }
            public string IsMarried { get; set; }
            public string Nation { get; set; }
            public string IDCard { get; set; }
            public string Phone { get; set; }
            public string InsuranceNo { get; set; }
            public string WorkUnit { get; set; }
            public string Occupation { get; set; }
            public string Address { get; set; }
            public string PastMedicalHistory { get; set; }
            public string DrugAllergyHistory { get; set; }
            public string GuardianName { get; set; }
            public string GuardianPhone { get; set; }
            public string GuardianRelationship { get; set; }
            public string GuardianAddress { get; set; }
            public string Remark { get; set; }
        }
        // 4. 取消挂号（新增恢复号源逻辑）
        public IActionResult Cancel(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 开启事务：保证状态更新和号源恢复原子性
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // 1. 查询挂号关联的排班ID + 检查状态/日期
                        string checkSql = @"
                    SELECT a.ScheduleID, a.[Status], s.Date 
                    FROM dbo.Appointments a 
                    LEFT JOIN dbo.Schedules s ON a.ScheduleID = s.ScheduleID 
                    WHERE a.AppointmentID = @AppointmentID";
                        SqlCommand checkCmd = new SqlCommand(checkSql, conn, transaction);
                        checkCmd.Parameters.AddWithValue("@AppointmentID", id);

                        SqlDataReader reader = checkCmd.ExecuteReader();
                        int scheduleID = 0;
                        string status = "";
                        DateTime scheduleDate = DateTime.MinValue;

                        if (reader.Read())
                        {
                            scheduleID = Convert.ToInt32(reader["ScheduleID"]);
                            status = reader["Status"].ToString();
                            scheduleDate = Convert.ToDateTime(reader["Date"]);
                        }
                        reader.Close();

                        // 2. 校验：仅允许取消未过期的「已预约」挂号
                        if (status != "已预约" || scheduleDate < DateTime.Today)
                        {
                            transaction.Rollback(); // 回滚事务
                            TempData["Error"] = "仅可取消未过期的「已预约」状态挂号！";
                            return RedirectToAction("MyAppointments");
                        }

                        // 3. 更新挂号状态为「已取消」
                        string updateSql = "UPDATE dbo.Appointments SET [Status] = N'已取消' WHERE AppointmentID = @AppointmentID";
                        SqlCommand updateCmd = new SqlCommand(updateSql, conn, transaction);
                        updateCmd.Parameters.AddWithValue("@AppointmentID", id);
                        int updateRows = updateCmd.ExecuteNonQuery();

                        if (updateRows > 0)
                        {
                            // 4. 恢复号源：该排班的剩余号源 +1（核心逻辑）
                            // 注：剩余号源 = 总号源 - 已使用号源，取消后已使用号源减少，剩余自动增加
                            // 无需手动修改Schedules表，查询时实时计算即可，这里仅记录操作日志（可选）
                            TempData["Success"] = "挂号已成功取消，号源已恢复！";
                        }
                        else
                        {
                            transaction.Rollback();
                            TempData["Error"] = "取消挂号失败，请重试！";
                            return RedirectToAction("MyAppointments");
                        }

                        // 提交事务
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // 异常回滚
                        TempData["Error"] = "取消挂号出错：" + ex.Message;
                        return RedirectToAction("MyAppointments");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "取消挂号出错：" + ex.Message;
            }

            return RedirectToAction("MyAppointments");
        }

        // 2. 提交挂号（新增预约）- 优化重复校验逻辑
        [HttpPost]
        public IActionResult Create(int ScheduleID, int PatientID, string Remark)
        {
            try
            {
                // 基础验证
                if (ScheduleID == 0 || PatientID == 0)
                {
                    TempData["Error"] = "请选择排班和患者！";
                    return RedirectToAction("Index");
                }

                // 检查排班是否有效（未过期、有剩余号源）
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 检查排班状态
                    string checkScheduleSql = @"
                SELECT s.MaxAppointments, 
                       -- 仅统计「已预约」状态的挂号数（排除已取消）
                       (SELECT COUNT(1) FROM dbo.Appointments a WHERE a.ScheduleID = s.ScheduleID AND a.[Status] = N'已预约') AS Used, 
                       s.Date
                FROM dbo.Schedules s
                WHERE s.ScheduleID = @ScheduleID";
                    SqlCommand checkCmd = new SqlCommand(checkScheduleSql, conn);
                    checkCmd.Parameters.AddWithValue("@ScheduleID", ScheduleID);
                    SqlDataReader reader = checkCmd.ExecuteReader();

                    if (!reader.Read())
                    {
                        TempData["Error"] = "该排班不存在！";
                        return RedirectToAction("Index");
                    }

                    int max = Convert.ToInt32(reader["MaxAppointments"]);
                    int used = reader["Used"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Used"]);
                    DateTime scheduleDate = Convert.ToDateTime(reader["Date"]);
                    reader.Close();

                    // 检查是否过期
                    if (scheduleDate < DateTime.Today)
                    {
                        TempData["Error"] = "该排班已过期，无法预约！";
                        return RedirectToAction("Index");
                    }

                    // 检查是否约满
                    if (used >= max)
                    {
                        TempData["Error"] = "该排班号源已约满，请选择其他排班！";
                        return RedirectToAction("Index");
                    }

                    // 核心优化：重复校验仅限制「已预约」状态，允许已取消的重新挂号
                    string checkPatientSql = @"
                SELECT COUNT(1) FROM dbo.Appointments 
                WHERE ScheduleID = @ScheduleID 
                AND PatientID = @PatientID 
                AND [Status] = N'已预约'; -- 仅校验未取消的预约
            ";
                    SqlCommand patientCmd = new SqlCommand(checkPatientSql, conn);
                    patientCmd.Parameters.AddWithValue("@ScheduleID", ScheduleID);
                    patientCmd.Parameters.AddWithValue("@PatientID", PatientID);
                    int patientCount = (int)patientCmd.ExecuteScalar();

                    if (patientCount > 0)
                    {
                        TempData["Error"] = "该患者已预约该排班（未取消），不可重复预约！";
                        return RedirectToAction("Index");
                    }

                    // 可选：提示患者该排班有过取消记录（提升体验）
                    string checkCanceledSql = @"
                SELECT COUNT(1) FROM dbo.Appointments 
                WHERE ScheduleID = @ScheduleID 
                AND PatientID = @PatientID 
                AND [Status] = N'已取消';
            ";
                    SqlCommand canceledCmd = new SqlCommand(checkCanceledSql, conn);
                    canceledCmd.Parameters.AddWithValue("@ScheduleID", ScheduleID);
                    canceledCmd.Parameters.AddWithValue("@PatientID", PatientID);
                    int canceledCount = (int)canceledCmd.ExecuteScalar();

                    if (canceledCount > 0)
                    {
                        TempData["Warning"] = "温馨提示：你曾取消过该排班的挂号，本次重新预约请确认就诊时间！";
                    }

                    // 插入预约记录
                    string insertSql = @"
                INSERT INTO dbo.Appointments (PatientID, ScheduleID, [Status], Remark, CreatedAt)
                VALUES (@PatientID, @ScheduleID, N'已预约', @Remark, GETDATE())";
                    SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@PatientID", PatientID);
                    insertCmd.Parameters.AddWithValue("@ScheduleID", ScheduleID);
                    insertCmd.Parameters.AddWithValue("@Remark", string.IsNullOrEmpty(Remark) ? DBNull.Value : (object)Remark);

                    int rows = insertCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = canceledCount > 0 ? "重新挂号成功！请按时到院就诊。" : "挂号成功！请按时到院就诊。";
                    }
                    else
                    {
                        TempData["Error"] = "挂号失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "挂号出错：" + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
