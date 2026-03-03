using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
namespace COMMSMVC.Controllers
{
    public class DoctorController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        //

        // 数据库连接字符串（替换为你的实际连接字符串）
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";

            // 1. 医生列表（查询）
            public IActionResult Index()
            {
                // 验证权限：仅管理员/医院工作人员可访问
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                    (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
                {
                    return RedirectToAction("Login", "Home");
                }

                DataTable dtDoctors = new DataTable();
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        // 关联科室表查询医生完整信息
                        string sql = @"
                        SELECT d.DoctorID, d.DoctorName, d.Title, d.DeptID, dp.DeptName, 
                               d.Phone, d.[Description], d.IsActive
                        FROM dbo.Doctors d
                        LEFT JOIN dbo.Departments dp ON d.DeptID = dp.DeptID
                        ORDER BY d.DoctorID DESC";
                        SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                        adapter.Fill(dtDoctors);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "加载医生列表失败：" + ex.Message;
                }

                return View(dtDoctors);
            }

            // 2. 新增医生（页面）
            public IActionResult Create()
            {
                // 验证权限
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                    (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
                {
                    return RedirectToAction("Login", "Home");
                }

                // 获取科室列表供下拉选择
                ViewBag.Departments = GetDepartments();
                return View();
            }

            // 3. 新增医生（提交）
            [HttpPost]
            public IActionResult Create(DoctorModel model)
            {
                try
                {
                    // 基础验证
                    if (string.IsNullOrEmpty(model.DoctorName) || model.DeptID == 0 || string.IsNullOrEmpty(model.Phone))
                    {
                        ViewBag.Error = "医生姓名、所属科室、联系电话为必填项！";
                        ViewBag.Departments = GetDepartments();
                        return View(model);
                    }

                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                        INSERT INTO dbo.Doctors (DoctorName, Title, DeptID, Phone, [Description], IsActive)
                        VALUES (@DoctorName, @Title, @DeptID, @Phone, @Description, @IsActive)";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@DoctorName", model.DoctorName);
                        cmd.Parameters.AddWithValue("@Title", string.IsNullOrEmpty(model.Title) ? DBNull.Value : (object)model.Title);
                        cmd.Parameters.AddWithValue("@DeptID", model.DeptID);
                        cmd.Parameters.AddWithValue("@Phone", model.Phone);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(model.Description) ? DBNull.Value : (object)model.Description);
                        cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? true:false); // 默认启用

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            TempData["Success"] = "医生信息新增成功！";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Error = "新增医生失败，请重试！";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "新增医生出错：" + ex.Message;
                }

                ViewBag.Departments = GetDepartments();
                return View(model);
            }

            // 4. 编辑医生（页面）
            public IActionResult Edit(int id)
            {
                // 验证权限
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                    (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
                {
                    return RedirectToAction("Login", "Home");
                }

                DoctorModel model = new DoctorModel();
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT * FROM dbo.Doctors WHERE DoctorID = @DoctorID";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@DoctorID", id);

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            model.DoctorID = Convert.ToInt32(reader["DoctorID"]);
                            model.DoctorName = reader["DoctorName"].ToString();
                            model.Title = reader["Title"].ToString();
                            model.DeptID = Convert.ToInt32(reader["DeptID"]);
                            model.Phone = reader["Phone"].ToString();
                            model.Description = reader["Description"].ToString();
                            model.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        }
                        else
                        {
                            TempData["Error"] = "未找到该医生信息！";
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "加载医生信息失败：" + ex.Message;
                }

                ViewBag.Departments = GetDepartments();
                return View(model);
            }

            // 5. 编辑医生（提交）
            [HttpPost]
            public IActionResult Edit(DoctorModel model)
            {
                try
                {
                    // 基础验证
                    if (string.IsNullOrEmpty(model.DoctorName) || model.DeptID == 0 || string.IsNullOrEmpty(model.Phone))
                    {
                        ViewBag.Error = "医生姓名、所属科室、联系电话为必填项！";
                        ViewBag.Departments = GetDepartments();
                        return View(model);
                    }

                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                        UPDATE dbo.Doctors 
                        SET DoctorName = @DoctorName, Title = @Title, DeptID = @DeptID, 
                            Phone = @Phone, [Description] = @Description, IsActive = @IsActive
                        WHERE DoctorID = @DoctorID";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                        cmd.Parameters.AddWithValue("@DoctorName", model.DoctorName);
                        cmd.Parameters.AddWithValue("@Title", string.IsNullOrEmpty(model.Title) ? DBNull.Value : (object)model.Title);
                        cmd.Parameters.AddWithValue("@DeptID", model.DeptID);
                        cmd.Parameters.AddWithValue("@Phone", model.Phone);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(model.Description) ? DBNull.Value : (object)model.Description);
                        cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? true:false);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            TempData["Success"] = "医生信息修改成功！";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Error = "修改医生信息失败，请重试！";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "修改医生信息出错：" + ex.Message;
                }

                ViewBag.Departments = GetDepartments();
                return View(model);
            }

            // 6. 删除医生
            public IActionResult Delete(int id)
            {
                // 验证权限
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                    (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
                {
                    return RedirectToAction("Login", "Home");
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        // 检查是否有关联的排班/预约记录
                        string checkSql = "SELECT COUNT(1) FROM dbo.Schedules WHERE DoctorID = @DoctorID";
                        SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                        checkCmd.Parameters.AddWithValue("@DoctorID", id);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            TempData["Error"] = "该医生存在关联的排班记录，无法删除！";
                            return RedirectToAction("Index");
                        }

                        // 执行删除
                        string deleteSql = "DELETE FROM dbo.Doctors WHERE DoctorID = @DoctorID";
                        SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
                        deleteCmd.Parameters.AddWithValue("@DoctorID", id);

                        int rows = deleteCmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            TempData["Success"] = "医生信息删除成功！";
                        }
                        else
                        {
                            TempData["Error"] = "删除医生信息失败，请重试！";
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "删除医生信息出错：" + ex.Message;
                }

                return RedirectToAction("Index");
            }

            // 辅助方法：获取科室列表
            private DataTable GetDepartments()
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DeptID, DeptName FROM dbo.Departments WHERE IsActive = 1 ORDER BY DeptID";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dt);
                }
                return dt;
            }
        }

        // 医生模型类
        public class DoctorModel
        {
        public int DoctorID { get; set; } = 0;
            public string DoctorName { get; set; }
            public string Title { get; set; }
            public int DeptID { get; set; }
            public string Phone { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
        }
    }
    //


