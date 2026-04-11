using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class AppointmentController : Controller
    {
        // 数据库连接字符串（替换为你的实际连接字符串）
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        private string baseUrl = "https://localhost:7190/api";
        public AppointmentController(IOptions<ApiConfig> apiConfig,IConfiguration configuration)
        {
            baseUrl = apiConfig.Value.BaseUrl; ;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult GetAppointmentIndex()
        {
            return RedirectToAction(nameof(VisitController.GetPatientAppointment), "Visit");

        }
        // 1. 挂号首页（选择排班+患者信息）
        [HttpGet]
        public IActionResult Index()
        {
            // 验证登录状态（患者/管理员均可访问）
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Home");
            }

            // 获取可预约的排班（未约满、未过期）
            DataTable dtAvailableSchedules = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 查询未过期且剩余号源>0的排班
                    string sql = @"
                                    SELECT 
                                        s.ScheduleID,
                                        d.DoctorName,
                                        dp.DeptName,
                                        s.Date,
                                        s.TimeSlot,
                                        s.MaxAppointments,
                                        COUNT(CASE WHEN a.Status <> N'已取消' THEN a.AppointmentID END) AS UsedAppointments,
                                        (s.MaxAppointments - COUNT(CASE WHEN a.Status <> N'已取消' THEN a.AppointmentID END)) AS Remaining
                                    FROM dbo.Schedules s
                                    LEFT JOIN dbo.Doctors d ON s.DoctorID = d.DoctorID
                                    LEFT JOIN dbo.Departments dp ON d.DeptID = dp.DeptID
                                    LEFT JOIN dbo.Appointments a ON s.ScheduleID = a.ScheduleID
                                    WHERE s.Date >= CONVERT(DATE, GETDATE()) 
                                      AND d.IsActive = 1
                                    GROUP BY s.ScheduleID, d.DoctorName, dp.DeptName, s.Date, s.TimeSlot, s.MaxAppointments
                                    HAVING (s.MaxAppointments - COUNT(CASE WHEN a.Status <> N'已取消' THEN a.AppointmentID END)) > 0
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
            if (HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "Doctor")
            {
                ViewBag.Patients = GetAllPatients();
            }
            else if (HttpContext.Session.GetString("Role") == "Patient")
            {
                // 患者仅能看到自己的信息
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                //ViewBag.Patients = GetPatientByUserId(userId);
                var dt = GetPatientByUserId(userId);
                ViewBag.Patients = dt;
                if (dt.Rows.Count <= 0) 
                {
                    //跳转患者创建https://localhost:7118/Patient/Register
                    return RedirectToAction("Register", "Patient");
                }
            }

            return View(dtAvailableSchedules);
        }

        // 2. 提交挂号（新增预约）
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
                        SELECT s.MaxAppointments, COUNT(a.AppointmentID) AS Used, s.Date
                        FROM dbo.Schedules s
                        LEFT JOIN dbo.Appointments a ON s.ScheduleID = a.ScheduleID
                        WHERE s.ScheduleID = @ScheduleID
                        GROUP BY s.MaxAppointments, s.Date";
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

                    // 检查患者是否已预约该排班（唯一约束）
                    string checkPatientSql = @"
                        SELECT COUNT(1) FROM dbo.Appointments 
                        WHERE ScheduleID = @ScheduleID AND PatientID = @PatientID";
                    SqlCommand patientCmd = new SqlCommand(checkPatientSql, conn);
                    patientCmd.Parameters.AddWithValue("@ScheduleID", ScheduleID);
                    patientCmd.Parameters.AddWithValue("@PatientID", PatientID);
                    int patientCount = (int)patientCmd.ExecuteScalar();

                    if (patientCount > 0)
                    {
                        TempData["Error"] = "该患者已预约该排班，不可重复预约！";
                        return RedirectToAction("Index");
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
                        TempData["Success"] = "挂号成功！请按时到院就诊。";
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

        // 3. 我的挂号列表
        public IActionResult MyAppointments()
        {
            // 验证登录
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Home");
            }

            DataTable dtAppointments = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "";
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    // 管理员/医护人员可查看所有挂号，患者仅查看自己的
                    if (HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "Staff")
                    {
                        sql = @"
                            SELECT a.AppointmentID, p.Name AS PatientName, d.DoctorName, dp.DeptName,
                                   s.Date, s.TimeSlot, a.[Status], a.Remark, a.CreatedAt
                            FROM dbo.Appointments a
                            LEFT JOIN dbo.Patients p ON a.PatientID = p.PatientID
                            LEFT JOIN dbo.Schedules s ON a.ScheduleID = s.ScheduleID
                            LEFT JOIN dbo.Doctors d ON s.DoctorID = d.DoctorID
                            LEFT JOIN dbo.Departments dp ON d.DeptID = dp.DeptID
                            ORDER BY a.CreatedAt DESC";
                    }
                    else if (HttpContext.Session.GetString("Role") == "Patient")
                    {
                        int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                        sql = @"
                            SELECT a.AppointmentID, p.Name AS PatientName, d.DoctorName, dp.DeptName,
                                   s.Date, s.TimeSlot, a.[Status], a.Remark, a.CreatedAt
                            FROM dbo.Appointments a
                            LEFT JOIN dbo.Patients p ON a.PatientID = p.PatientID
                            LEFT JOIN dbo.Schedules s ON a.ScheduleID = s.ScheduleID
                            LEFT JOIN dbo.Doctors d ON s.DoctorID = d.DoctorID
                            LEFT JOIN dbo.Departments dp ON d.DeptID = dp.DeptID
                            WHERE p.UserId = @UserId
                            ORDER BY a.CreatedAt DESC";
                        cmd.Parameters.AddWithValue("@UserId", userId);
                    }

                    cmd.CommandText = sql;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dtAppointments);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "加载挂号列表失败：" + ex.Message;
            }

            return View(dtAppointments);
        }

        // 4. 取消挂号
        public IActionResult Cancel(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // 检查挂号状态（已完成/已取消的不能再取消）
                    string checkSql = "SELECT [Status], s.Date FROM dbo.Appointments a LEFT JOIN dbo.Schedules s ON a.ScheduleID = s.ScheduleID WHERE a.AppointmentID = @AppointmentID";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@AppointmentID", id);
                    SqlDataReader reader = checkCmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        DateTime scheduleDate = Convert.ToDateTime(reader["Date"]);

                        // 检查是否已过期/已完成
                        if (status != "已预约" || scheduleDate < DateTime.Today)
                        {
                            TempData["Error"] = "仅可取消未过期的「已预约」状态挂号！";
                            return RedirectToAction("MyAppointments");
                        }
                    }
                    reader.Close();

                    // 更新状态为取消
                    string updateSql = "UPDATE dbo.Appointments SET [Status] = N'已取消' WHERE AppointmentID = @AppointmentID";
                    SqlCommand updateCmd = new SqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@AppointmentID", id);

                    int rows = updateCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "挂号已成功取消！";
                    }
                    else
                    {
                        TempData["Error"] = "取消挂号失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "取消挂号出错：" + ex.Message;
            }

            return RedirectToAction("MyAppointments");
        }
        //5.恢复挂号
        public IActionResult Restore(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // 1. 查询当前挂号状态及排班信息
                    string checkSql = @"
                SELECT a.Status, s.Date, s.ScheduleID, s.MaxAppointments
                FROM dbo.Appointments a
                LEFT JOIN dbo.Schedules s ON a.ScheduleID = s.ScheduleID
                WHERE a.AppointmentID = @AppointmentID";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@AppointmentID", id);
                    SqlDataReader reader = checkCmd.ExecuteReader();

                    if (!reader.Read())
                    {
                        reader.Close();
                        TempData["Error"] = "未找到该挂号记录！";
                        return RedirectToAction("MyAppointments");
                    }

                    string status = reader["Status"].ToString();
                    DateTime scheduleDate = Convert.ToDateTime(reader["Date"]);
                    int scheduleId = Convert.ToInt32(reader["ScheduleID"]);
                    int maxAppointments = Convert.ToInt32(reader["MaxAppointments"]);
                    reader.Close();

                    // 2. 验证是否可恢复：状态必须为“已取消”，且排班日期未过期
                    if (status != "已取消")
                    {
                        TempData["Error"] = "只有已取消的挂号才能恢复！";
                        return RedirectToAction("MyAppointments");
                    }
                    if (scheduleDate < DateTime.Today)
                    {
                        TempData["Error"] = "排班日期已过期，无法恢复挂号！";
                        return RedirectToAction("MyAppointments");
                    }

                    // 3. 检查当前排班的剩余号源（仅统计状态为“已预约”的挂号）
                    string countSql = @"
                SELECT COUNT(1) FROM dbo.Appointments
                WHERE ScheduleID = @ScheduleID AND [Status] = N'已预约'";
                    SqlCommand countCmd = new SqlCommand(countSql, conn);
                    countCmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                    int used = (int)countCmd.ExecuteScalar();
                    int remaining = maxAppointments - used;

                    if (remaining <= 0)
                    {
                        TempData["Error"] = "该排班号源已满，无法恢复挂号！";
                        return RedirectToAction("MyAppointments");
                    }

                    // 4. 更新状态为“已预约”
                    string updateSql = "UPDATE dbo.Appointments SET [Status] = N'已预约' WHERE AppointmentID = @AppointmentID";
                    SqlCommand updateCmd = new SqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@AppointmentID", id);
                    int rows = updateCmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        TempData["Success"] = "挂号已成功恢复！";
                    }
                    else
                    {
                        TempData["Error"] = "恢复挂号失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "恢复挂号出错：" + ex.Message;
            }

            return RedirectToAction("MyAppointments");
        }
        // 辅助方法：获取所有患者
        private DataTable GetAllPatients()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT PatientID, Name FROM dbo.Patients ORDER BY Name";
                SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                adapter.Fill(dt);
            }
            return dt;
        }

        // 辅助方法：根据用户ID获取患者
        private DataTable GetPatientByUserId(int userId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT PatientID, Name FROM dbo.Patients WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }
    }
}