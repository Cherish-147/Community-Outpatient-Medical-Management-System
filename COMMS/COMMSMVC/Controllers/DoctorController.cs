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
        // 新增：排班模型类（放在DoctorController同级）
        public class ScheduleModel
        {
            public int ScheduleID { get; set; }
            public int DoctorID { get; set; }
            public string DoctorName { get; set; } // 用于展示医生姓名
            public DateTime Date { get; set; } = DateTime.Today; // 默认当天
            public string TimeSlot { get; set; } // 上午/下午
            public int MaxAppointments { get; set; } = 10; // 默认最大预约数
        }

        // DoctorController中新增排班相关方法
        // 1. 排班列表（查询）
        public IActionResult Schedule()
        {
            // 验证权限：仅管理员/医院工作人员可访问
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
            {
                return RedirectToAction("Login", "Home");
            }

            DataTable dtSchedules = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 关联医生表查询排班完整信息
                    string sql = @"
                SELECT s.ScheduleID, s.DoctorID, d.DoctorName, s.Date, s.TimeSlot, s.MaxAppointments
                FROM dbo.Schedules s
                LEFT JOIN dbo.Doctors d ON s.DoctorID = d.DoctorID
                WHERE d.IsActive = 1
                ORDER BY s.Date DESC, s.TimeSlot";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dtSchedules);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "加载排班列表失败：" + ex.Message;
            }

            ViewBag.Doctors = GetActiveDoctors(); // 获取可用医生列表
            return View(dtSchedules);
        }
        #region 医生排班
        // 2. 新增排班（提交）
        [HttpPost]
        public IActionResult AddSchedule(ScheduleModel model)
        {
            try
            {
                // 基础验证
                if (model.DoctorID == 0 || string.IsNullOrEmpty(model.TimeSlot) || model.MaxAppointments <= 0)
                {
                    TempData["Error"] = "医生、时段、最大预约数为必填项，且最大预约数必须大于0！";
                    return RedirectToAction("Schedule");
                }

                // 检查是否重复排班（同一医生同一天同一时段）
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string checkSql = @"
                SELECT COUNT(1) FROM dbo.Schedules 
                WHERE DoctorID = @DoctorID AND Date = @Date AND TimeSlot = @TimeSlot";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                    checkCmd.Parameters.AddWithValue("@Date", model.Date);
                    checkCmd.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        TempData["Error"] = $"该医生{model.Date:yyyy-MM-dd}{model.TimeSlot}已排班，不可重复添加！";
                        return RedirectToAction("Schedule");
                    }

                    // 插入排班数据
                    string insertSql = @"
                INSERT INTO dbo.Schedules (DoctorID, Date, TimeSlot, MaxAppointments)
                VALUES (@DoctorID, @Date, @TimeSlot, @MaxAppointments)";
                    SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                    insertCmd.Parameters.AddWithValue("@Date", model.Date);
                    insertCmd.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
                    insertCmd.Parameters.AddWithValue("@MaxAppointments", model.MaxAppointments);

                    int rows = insertCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "排班添加成功！";
                    }
                    else
                    {
                        TempData["Error"] = "添加排班失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "添加排班出错：" + ex.Message;
            }

            return RedirectToAction("Schedule");
        }

        // 3. 编辑排班（提交）
        [HttpPost]
        public IActionResult EditSchedule(ScheduleModel model)
        {
            try
            {
                // 基础验证
                if (model.ScheduleID == 0 || model.DoctorID == 0 || string.IsNullOrEmpty(model.TimeSlot) || model.MaxAppointments <= 0)
                {
                    TempData["Error"] = "排班ID、医生、时段、最大预约数为必填项，且最大预约数必须大于0！";
                    return RedirectToAction("Schedule");
                }

                // 检查编辑后是否重复（排除自身）
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string checkSql = @"
                SELECT COUNT(1) FROM dbo.Schedules 
                WHERE DoctorID = @DoctorID AND Date = @Date AND TimeSlot = @TimeSlot AND ScheduleID != @ScheduleID";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                    checkCmd.Parameters.AddWithValue("@Date", model.Date);
                    checkCmd.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
                    checkCmd.Parameters.AddWithValue("@ScheduleID", model.ScheduleID);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        TempData["Error"] = $"该医生{model.Date:yyyy-MM-dd}{model.TimeSlot}已排班，不可重复！";
                        return RedirectToAction("Schedule");
                    }

                    // 更新排班数据
                    string updateSql = @"
                UPDATE dbo.Schedules 
                SET DoctorID = @DoctorID, Date = @Date, TimeSlot = @TimeSlot, MaxAppointments = @MaxAppointments
                WHERE ScheduleID = @ScheduleID";
                    SqlCommand updateCmd = new SqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@ScheduleID", model.ScheduleID);
                    updateCmd.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                    updateCmd.Parameters.AddWithValue("@Date", model.Date);
                    updateCmd.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
                    updateCmd.Parameters.AddWithValue("@MaxAppointments", model.MaxAppointments);

                    int rows = updateCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "排班修改成功！";
                    }
                    else
                    {
                        TempData["Error"] = "修改排班失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "修改排班出错：" + ex.Message;
            }

            return RedirectToAction("Schedule");
        }

        // 4. 删除排班
        public IActionResult DeleteSchedule(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 检查是否有关联的预约记录
                    string checkSql = @"
                SELECT COUNT(1) FROM dbo.Appointments a
                LEFT JOIN dbo.Schedules s ON a.ScheduleID = s.ScheduleID
                WHERE s.ScheduleID = @ScheduleID";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@ScheduleID", id);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        TempData["Error"] = "该排班存在关联的预约记录，无法删除！";
                        return RedirectToAction("Schedule");
                    }

                    // 执行删除
                    string deleteSql = "DELETE FROM dbo.Schedules WHERE ScheduleID = @ScheduleID";
                    SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
                    deleteCmd.Parameters.AddWithValue("@ScheduleID", id);

                    int rows = deleteCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "排班删除成功！";
                    }
                    else
                    {
                        TempData["Error"] = "删除排班失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "删除排班出错：" + ex.Message;
            }

            return RedirectToAction("Schedule");
        }

        // 辅助方法：获取激活的医生列表
        private DataTable GetActiveDoctors()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT DoctorID, DoctorName FROM dbo.Doctors WHERE IsActive = 1 ORDER BY DoctorName";
                SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                adapter.Fill(dt);
            }
            return dt;
        }
        //

#endregion
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


