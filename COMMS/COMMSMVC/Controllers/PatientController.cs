using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False";

        // 患者首页
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
        public IActionResult MyPayments()
        {
            // 登录验证
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                HttpContext.Session.GetString("Role") != "Patient")
            {
                return RedirectToAction("Login", "Home");
            }
            ViewData["Title"] = "缴费记录 - 社区门诊患者中心";
            return View();
        }

        //注册账号后,判断是否Patient存在 UserId ，如果不存在强制填写患者注册
        // 新增：患者注册页面展示（无需登录验证，允许未注册用户访问）
        public IActionResult Register()
        {
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
                    string userId = HttpContext.Session.GetString("UserID") ?? "0";
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
    }
}
