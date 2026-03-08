using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class ProfileController : Controller
    {
        // 数据库连接字符串（替换为你的实际值）
        private readonly string _connectionString = "Server=你的服务器;Database=Community-Outpatient-Medical-Management-System;User Id=账号;Password=密码;TrustServerCertificate=True;";

        // 个人信息首页（查看+编辑）
        public IActionResult Index()
        {
            // 验证登录状态
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            string role = HttpContext.Session.GetString("Role");
            ViewData["Role"] = role;

            // 存储用户基础信息
            ViewBag.UserInfo = new
            {
                UserID = userId,
                Username = HttpContext.Session.GetString("Username"),
                RoleName = role == "Admin" ? "管理员" : (role == "Staff" ? "医护人员" : "患者")
            };

            // 不同角色加载不同的详细信息
            if (role == "Patient")
            {
                // 患者：加载患者详细信息
                ViewBag.PatientInfo = GetPatientInfoByUserId(userId);
            }
            else
            {
                // 管理员/医护人员：加载账号基础信息
                ViewBag.StaffInfo = GetStaffInfoByUserId(userId);
            }

            return View();
        }

        // 修改密码（提交）
        [HttpPost]
        public IActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                // 基础验证
                if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
                {
                    TempData["Error"] = "请完整填写所有密码字段！";
                    return RedirectToAction("Index");
                }

                if (NewPassword != ConfirmPassword)
                {
                    TempData["Error"] = "新密码和确认密码不一致！";
                    return RedirectToAction("Index");
                }

                if (NewPassword.Length < 6)
                {
                    TempData["Error"] = "新密码长度不能少于6位！";
                    return RedirectToAction("Index");
                }

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 1. 验证原密码
                    string checkPwdSql = "SELECT Password FROM dbo.Users WHERE UserID = @UserID";
                    SqlCommand checkCmd = new SqlCommand(checkPwdSql, conn);
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    string dbPwd = checkCmd.ExecuteScalar()?.ToString();

                    if (dbPwd != OldPassword) // 注：实际项目建议加密存储密码，此处简化
                    {
                        TempData["Error"] = "原密码错误！";
                        return RedirectToAction("Index");
                    }

                    // 2. 更新新密码
                    string updateSql = "UPDATE dbo.Users SET Password = @NewPassword WHERE UserID = @UserID";
                    SqlCommand updateCmd = new SqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@NewPassword", NewPassword);
                    updateCmd.Parameters.AddWithValue("@UserID", userId);

                    int rows = updateCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "密码修改成功！请重新登录。";
                        // 可选：退出登录
                        // HttpContext.Session.Clear();
                        // return RedirectToAction("Login", "Home");
                    }
                    else
                    {
                        TempData["Error"] = "密码修改失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "修改密码出错：" + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 更新个人信息（提交）
        [HttpPost]
        public IActionResult UpdateInfo(string Name, string Phone, string? Gender, string IDCard)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                string role = HttpContext.Session.GetString("Role");

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (role == "Patient")
                    {
                        // 患者：更新患者表信息
                        string updateSql = @"
                            UPDATE dbo.Patients 
                            SET Name = @Name, Phone = @Phone, Gender = @Gender, Age = @Age, IDCard = @IDCard
                            WHERE UserId = @UserId";
                        SqlCommand cmd = new SqlCommand(updateSql, conn);
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Phone", Phone);
                        cmd.Parameters.AddWithValue("@Gender", Gender ??"男");
                      
                        cmd.Parameters.AddWithValue("@IDCard", IDCard);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            TempData["Success"] = "患者信息更新成功！";
                        }
                        else
                        {
                            TempData["Error"] = "患者信息更新失败，请重试！";
                        }
                    }
                    else
                    {
                        // 管理员/医护人员：更新用户表基础信息
                        string updateSql = @"
                            UPDATE dbo.Users 
                            SET Name = @Name, Phone = @Phone 
                            WHERE UserID = @UserID";
                        SqlCommand cmd = new SqlCommand(updateSql, conn);
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Phone", Phone);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            TempData["Success"] = "个人信息更新成功！";
                        }
                        else
                        {
                            TempData["Error"] = "个人信息更新失败，请重试！";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "更新信息出错：" + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 辅助方法：根据用户ID获取患者信息
        private DataRow GetPatientInfoByUserId(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM dbo.Patients WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        // 辅助方法：根据用户ID获取管理员/医护人员信息
        private DataRow GetStaffInfoByUserId(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM dbo.Users WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }
    }
}