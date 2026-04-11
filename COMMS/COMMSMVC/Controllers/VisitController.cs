using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class VisitController : Controller
    {
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
       private string baseUrl = "https://localhost:7190/api";
        public VisitController(IOptions<ApiConfig> apiConfig,IConfiguration configuration)
        {
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #region// 模拟数据
        private List<CheckItem> _checkItems = new List<CheckItem>
        {
            new CheckItem { CheckItemID = 1, Name = "血常规", Description = "全血细胞计数检查", Price = 30.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 2, Name = "尿常规", Description = "尿液常规检查", Price = 20.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 3, Name = "心电图", Description = "心脏电活动检查", Price = 50.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 4, Name = "B超", Description = "腹部超声波检查", Price = 120.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 5, Name = "CT扫描", Description = "计算机断层扫描", Price = 300.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 6, Name = "X光胸片", Description = "胸部X光检查", Price = 80.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 7, Name = "肝功能", Description = "肝脏功能检查", Price = 60.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 8, Name = "肾功能", Description = "肾脏功能检查", Price = 55.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 9, Name = "血糖检测", Description = "血糖水平检查", Price = 15.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CheckItem { CheckItemID = 10, Name = "血脂检测", Description = "血脂四项检查", Price = 40.00m, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        };
        #endregion
        public virtual void GetCheckItemInfo(out List<CheckItem> checkItems)//方法-获取所有检查单清单
        {
            checkItems = new List<CheckItem>();
            string sql = @"
                        SELECT [CheckItemID]
                              ,[Name]
                              ,[Description]
                              ,[Price]
                              ,[IsActive]
                              ,[CreatedAt]
                              ,[UpdatedAt]
                          FROM [CheckItems]
                        ";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new CheckItem
                        {
                            CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                                          ? null
                                          : reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        };
                        checkItems.Add(item);
                    }
                }
            }
        }
        #region// 模拟预约数据（从您提供的 CheckOrders 表中提取 AppointmentID）
        private List<int> _appointmentIds = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        #endregion

        public virtual void GetAppointmentIds(out List<int> appointmentIds)//方法，获取预约Id List
        {
            appointmentIds = [];
            var appointments = new List<AppointmentViewModel>();
            GetPatientAppointmentInfo(out appointments);
            //法一：循环
            foreach (var item in appointments)
            {
                appointmentIds.Add(item.AppointmentID);
            }
            //法二:LINQ
            appointmentIds = appointments.Select(a => a.AppointmentID).ToList();
        }
        #region 就诊管理
        public IActionResult Index()
        {
            var appointments = new List<AppointmentViewModel>
    {
        new AppointmentViewModel { AppointmentID = 1, PatientID = 1, PatientName = "王五", ScheduleID = 1, Status = "已预约", CreatedAt = new DateTime(2024,1,10), Remark = "常规检查" },
        // ... 继续添加其他示例数据（为简洁省略，实际从数据库获取）
    };
            return View(appointments);

        }
        public virtual void GetTodayPatientAppointmentInfo(out List<AppointmentViewModel> appointments) //方法-获取今天挂号人
        {
            appointments = new List<AppointmentViewModel>();
            //获取今天挂号人
            string sql = @"
            select a.AppointmentID,a.PatientID,p.[Name],s.ScheduleID,a.[Status],a.CreatedAt,a.Remark
            ,s.TimeSlot,d.DoctorID,d.DoctorName
            from Appointments a 
            inner join Schedules s on a.ScheduleID =s.ScheduleID
            inner join Patients p on a.PatientID = p.PatientID
            inner join Doctors d on d.DoctorID=s.DoctorID
            where s.[Date] >= CAST(GETDATE() AS DATE) 
                  AND s.[Date] <  DATEADD(DAY, 1, CAST(GETDATE() AS DATE))";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appointments.Add(new AppointmentViewModel
                        {
                            AppointmentID = reader.GetInt32(0),               // AppointmentID
                            PatientID = reader.GetInt32(1),                    // PatientID
                            PatientName = reader.GetString(2),                 // Name
                            ScheduleID = reader.GetInt32(3),                   // ScheduleID
                            Status = reader.GetString(4),                      // Status
                            CreatedAt = reader.GetDateTime(5),                 // CreatedAt
                            Remark = reader.IsDBNull(6) ? null : reader.GetString(6), // Remark
                            TimeSlot = reader.GetString(7),                    // TimeSlot
                            DoctorID = reader.GetInt32(8),                     // DoctorID
                            DoctorName = reader.GetString(9)                   // DoctorName
                        });
                    }
                }
            }
        }
        //查看当天挂号患者
        public IActionResult GetTodayPatientAppointment()//控制器-获取今天挂号人
        {
            var appointments = new List<AppointmentViewModel>();
            GetTodayPatientAppointmentInfo(out appointments);

            return View(appointments);
        }

        public IActionResult GetOnedayPatientAppointment(DateTime? startDate, DateTime? endDate, out List<AppointmentViewModel> appointments)//控制器获取某天挂号人
        {
            // 如果未输入日期，默认查询今天
            if (!startDate.HasValue)
                startDate = DateTime.Today;
            if (!endDate.HasValue)
                endDate = DateTime.Today;
            // 注意：结束日期取用户所选日期的下一天，以保证包含所选结束日期的全天数据
            DateTime start = startDate.Value.Date; // 确保时间部分为 00:00:00
            DateTime end = endDate.Value.Date.AddDays(1); // 结束日期的下一天 00:00:00

            appointments = new List<AppointmentViewModel>();
            //获取今天挂号人
            string sql = @"
            select a.AppointmentID,a.PatientID,p.[Name],s.ScheduleID,a.[Status],a.CreatedAt,a.Remark
            ,s.TimeSlot,d.DoctorID,d.DoctorName
            from Appointments a 
            inner join Schedules s on a.ScheduleID =s.ScheduleID
            inner join Patients p on a.PatientID = p.PatientID
            inner join Doctors d on d.DoctorID=s.DoctorID
            where s.[Date] >= CAST(GETDATE() AS DATE) 
                  AND s.[Date] <  DATEADD(DAY, 1, CAST(GETDATE() AS DATE))";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appointments.Add(new AppointmentViewModel
                        {
                            AppointmentID = reader.GetInt32(0),               // AppointmentID
                            PatientID = reader.GetInt32(1),                    // PatientID
                            PatientName = reader.GetString(2),                 // Name
                            ScheduleID = reader.GetInt32(3),                   // ScheduleID
                            Status = reader.GetString(4),                      // Status
                            CreatedAt = reader.GetDateTime(5),                 // CreatedAt
                            Remark = reader.IsDBNull(6) ? null : reader.GetString(6), // Remark
                            TimeSlot = reader.GetString(7),                    // TimeSlot
                            DoctorID = reader.GetInt32(8),                     // DoctorID
                            DoctorName = reader.GetString(9)                   // DoctorName
                        });
                    }
                }
            }

            return View(appointments);
        }

        public virtual void GetPatientAppointmentInfo(out List<AppointmentViewModel> appointments)//方法-查看全部患者挂号
        {
            appointments = new List<AppointmentViewModel>();
            string sql = @"
                        select a.AppointmentID,a.PatientID,p.[Name],s.ScheduleID,a.[Status],a.CreatedAt,a.Remark
                        ,s.TimeSlot,d.DoctorID,d.DoctorName
                        from Appointments a 
                        inner join Schedules s on a.ScheduleID =s.ScheduleID
                        inner join Patients p on a.PatientID = p.PatientID
                        inner join Doctors d on d.DoctorID=s.DoctorID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appointments.Add(new AppointmentViewModel
                        {
                            AppointmentID = reader.GetInt32(0),               // AppointmentID
                            PatientID = reader.GetInt32(1),                    // PatientID
                            PatientName = reader.GetString(2),                 // Name
                            ScheduleID = reader.GetInt32(3),                   // ScheduleID
                            Status = reader.GetString(4),                      // Status
                            CreatedAt = reader.GetDateTime(5),                 // CreatedAt
                            Remark = reader.IsDBNull(6) ? null : reader.GetString(6), // Remark
                            TimeSlot = reader.GetString(7),                    // TimeSlot
                            DoctorID = reader.GetInt32(8),                     // DoctorID
                            DoctorName = reader.GetString(9)                   // DoctorName
                        });
                    }
                }
            }
        }

        [HttpGet("/Visit/GetPatientAppointmentById/{appointmentID}")]
        public IActionResult GetPatientAppointmentById(int appointmentID)//控制器-这个患者挂号信息
        {
            var appointment = new List<AppointmentViewModel>();
            GetOnePatientAppointmentInfo(appointmentID, out appointment);
            return View(appointment);
        }
        public virtual void GetOnePatientAppointmentInfo(int appointmentID, out List<AppointmentViewModel> appointment)//方法-查看一个患者挂号
        {
            appointment = [];
            string sql = @"
                        select a.AppointmentID,a.PatientID,p.[Name],s.ScheduleID,a.[Status],a.CreatedAt,a.Remark
                        ,s.TimeSlot,d.DoctorID,d.DoctorName
                        from Appointments a 
                        inner join Schedules s on a.ScheduleID =s.ScheduleID
                        inner join Patients p on a.PatientID = p.PatientID
                        inner join Doctors d on d.DoctorID=s.DoctorID
                        where  a.AppointmentID =@AppointmentID
";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        appointment.Add(new AppointmentViewModel
                        {
                            AppointmentID = reader.GetInt32(0),               // AppointmentID
                            PatientID = reader.GetInt32(1),                    // PatientID
                            PatientName = reader.GetString(2),                 // Name
                            ScheduleID = reader.GetInt32(3),                   // ScheduleID
                            Status = reader.GetString(4),                      // Status
                            CreatedAt = reader.GetDateTime(5),                 // CreatedAt
                            Remark = reader.IsDBNull(6) ? null : reader.GetString(6), // Remark
                            TimeSlot = reader.GetString(7),                    // TimeSlot
                            DoctorID = reader.GetInt32(8),                     // DoctorID
                            DoctorName = reader.GetString(9)                   // DoctorName
                        });
                    }
                }
            }
        }
        public IActionResult GetPatientAppointment() //控制器--查看全部患者挂号
        {
            #region 挂号患者 数据
            var appointments = new List<AppointmentViewModel>
{
    new AppointmentViewModel
    {
        AppointmentID = 1,
        PatientID = 1,
        PatientName = "王五",
        ScheduleID = 1,
        Status = "已预约",
        CreatedAt = DateTime.Parse("2024-01-10 00:00:00.000"),
        Remark = "常规检查",
        TimeSlot = "上午",
        DoctorID = 1,
        DoctorName = "张医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 2,
        PatientID = 2,
        PatientName = "赵六",
        ScheduleID = 2,
        Status = "已到诊",
        CreatedAt = DateTime.Parse("2024-01-10 00:00:00.000"),
        Remark = "术后复查",
        TimeSlot = "下午",
        DoctorID = 2,
        DoctorName = "李医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 3,
        PatientID = 3,
        PatientName = "钱七",
        ScheduleID = 3,
        Status = "已看诊",
        CreatedAt = DateTime.Parse("2024-01-10 00:00:00.000"),
        Remark = "感冒发烧",
        TimeSlot = "上午",
        DoctorID = 3,
        DoctorName = "王医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 4,
        PatientID = 4,
        PatientName = "孙八",
        ScheduleID = 4,
        Status = "已完成",
        CreatedAt = DateTime.Parse("2024-01-09 00:00:00.000"),
        Remark = "妇科检查",
        TimeSlot = "下午",
        DoctorID = 4,
        DoctorName = "赵医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 5,
        PatientID = 5,
        PatientName = "吴十",
        ScheduleID = 5,
        Status = "取消",
        CreatedAt = DateTime.Parse("2024-01-09 00:00:00.000"),
        Remark = "患者临时有事",
        TimeSlot = "上午",
        DoctorID = 5,
        DoctorName = "钱医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 6,
        PatientID = 6,
        PatientName = "郑十一",
        ScheduleID = 6,
        Status = "已预约",
        CreatedAt = DateTime.Parse("2024-01-11 00:00:00.000"),
        Remark = "耳鼻喉检查",
        TimeSlot = "下午",
        DoctorID = 6,
        DoctorName = "孙医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 7,
        PatientID = 7,
        PatientName = "王十二",
        ScheduleID = 7,
        Status = "已到诊",
        CreatedAt = DateTime.Parse("2024-01-11 00:00:00.000"),
        Remark = "牙齿疼痛",
        TimeSlot = "上午",
        DoctorID = 7,
        DoctorName = "周医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 8,
        PatientID = 1,
        PatientName = "王五",
        ScheduleID = 11,
        Status = "已取消",
        CreatedAt = DateTime.Parse("2026-03-04 01:27:45.347"),
        Remark = "发骚", // 注意：此处保留了原始数据中的文本
        TimeSlot = "下午",
        DoctorID = 2,
        DoctorName = "李医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 9,
        PatientID = 13,
        PatientName = "2", // 原始数据中 Name 列为 "2"
        ScheduleID = 13,
        Status = "已预约",
        CreatedAt = DateTime.Parse("2026-03-08 11:37:24.197"),
        Remark = "1", // 原始数据中 Remark 列为 "1"
        TimeSlot = "上午",
        DoctorID = 11,
        DoctorName = "杨医生"
    },
    new AppointmentViewModel
    {
        AppointmentID = 10,
        PatientID = 13,
        PatientName = "2", // 原始数据中 Name 列为 "2"
        ScheduleID = 14,
        Status = "已取消",
        CreatedAt = DateTime.Parse("2026-03-08 11:41:44.880"),
        Remark = "扭到脚了",
        TimeSlot = "下午",
        DoctorID = 2,
        DoctorName = "李医生"
    }
};
            #endregion
            GetPatientAppointmentInfo(out appointments);

            return View(appointments);
        }
        
        public IActionResult GetPatients()//控制器--查看患者所有信息
        {
            #region 所有患者信息
            var patients = new List<Patient>
{
    new Patient
    {
        PatientID = 1,
        UserId = 3,
        Name = "王五",
        Birthday = DateTime.Parse("2000-01-17 18:31:42.000"),
        Gender = "男",
        IDCard = "110101199001011234",
        Phone = "13800138002",
        InsuranceNo = "BJ123456789",
        CreatedAt = DateTime.Parse("2024-01-01 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 1,
        UserId = 3,
        Name = "王五",
        Birthday = DateTime.Parse("2000-01-17 18:31:42.000"),
        Gender = "男",
        IDCard = "110101199001011234",
        Phone = "13800138002",
        InsuranceNo = "BJ123456789",
        CreatedAt = DateTime.Parse("2024-01-01 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 2,
        UserId = 4,
        Name = "赵六",
        Birthday = null,
        Gender = "F",
        IDCard = "110101199002022345",
        Phone = "13800138003",
        InsuranceNo = "BJ123456790",
        CreatedAt = DateTime.Parse("2024-01-02 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 3,
        UserId = 5,
        Name = "钱七",
        Birthday = null,
        Gender = "F",
        IDCard = "110101199003033456",
        Phone = "13800138004",
        InsuranceNo = "BJ123456791",
        CreatedAt = DateTime.Parse("2024-01-03 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 4,
        UserId = 6,
        Name = "孙八",
        Birthday = null,
        Gender = "M",
        IDCard = "110101199004044567",
        Phone = "13800138005",
        InsuranceNo = "BJ123456792",
        CreatedAt = DateTime.Parse("2024-01-04 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 5,
        UserId = 8,
        Name = "吴十",
        Birthday = null,
        Gender = "M",
        IDCard = "110101199005055678",
        Phone = "13800138007",
        InsuranceNo = "BJ123456793",
        CreatedAt = DateTime.Parse("2024-01-05 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 6,
        UserId = 9,
        Name = "郑十一",
        Birthday = null,
        Gender = "F",
        IDCard = "110101199006066789",
        Phone = "13800138008",
        InsuranceNo = "BJ123456794",
        CreatedAt = DateTime.Parse("2024-01-06 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 7,
        UserId = 10,
        Name = "王十二",
        Birthday = null,
        Gender = "M",
        IDCard = "110101199007077890",
        Phone = "13800138009",
        InsuranceNo = "BJ123456795",
        CreatedAt = DateTime.Parse("2024-01-07 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 13,
        UserId = 14,
        Name = "2",
        Birthday = DateTime.Parse("2026-03-09 00:00:00.000"),
        Gender = "男",
        IDCard = "1234567787",
        Phone = "13800138109",
        InsuranceNo = "1234556778",
        CreatedAt = DateTime.Parse("2026-03-08 11:37:15.283"),
        UpdatedAt = DateTime.Parse("2026-03-08 11:37:15"),
        IsMarried = true,
        Nation = "汉",
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    },
    new Patient
    {
        PatientID = 13,
        UserId = 14,
        Name = "2",
        Birthday = DateTime.Parse("2026-03-09 00:00:00.000"),
        Gender = "男",
        IDCard = "1234567787",
        Phone = "13800138109",
        InsuranceNo = "1234556778",
        CreatedAt = DateTime.Parse("2026-03-08 11:37:15.283"),
        UpdatedAt = DateTime.Parse("2026-03-08 11:37:15"),
        IsMarried = true,
        Nation = "汉",
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    }
};
            #endregion
            GetPatientsInfo(out patients);
            return View(patients);

        }
        public virtual void GetPatientsInfo(out List<Patient> patients)//方法，获取所有患者信息
        {
            patients = new List<Patient>();
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
  FROM [Patients]
";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var patient = new Patient
                        {
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("Gender"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Gender")),
                            IDCard = reader.IsDBNull(reader.GetOrdinal("IDCard"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("IDCard")),
                            Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Phone")),
                            InsuranceNo = reader.IsDBNull(reader.GetOrdinal("InsuranceNo"))
                                          ? null
                                          : reader.GetString(reader.GetOrdinal("InsuranceNo")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried"))
                                        ? (bool?)null
                                        : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                            Nation = reader.IsDBNull(reader.GetOrdinal("Nation"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Nation")),
                            WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit"))
                                       ? null
                                       : reader.GetString(reader.GetOrdinal("WorkUnit")),
                            Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation"))
                                         ? null
                                         : reader.GetString(reader.GetOrdinal("Occupation")),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address"))
                                      ? null
                                      : reader.GetString(reader.GetOrdinal("Address")),
                            PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                            DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                            GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName"))
                                           ? null
                                           : reader.GetString(reader.GetOrdinal("GuardianName")),
                            GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship"))
                                                   ? null
                                                   : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                            GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress"))
                                              ? null
                                              : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                            GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                            Remark = reader.IsDBNull(reader.GetOrdinal("Remark"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Remark"))
                        };
                        patients.Add(patient);
                    }
                }
            }
        }//获取所有患者信息


        public virtual void GetPatientByIdInfo(int patientId, out List<Patient> patientList)//方法--获取某个患者信息
        {
            patientList = new List<Patient>();
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
  FROM [Patients]
Where PatientID =@PatientID
";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PatientId", patientId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var patient = new Patient
                        {
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("Gender"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Gender")),
                            IDCard = reader.IsDBNull(reader.GetOrdinal("IDCard"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("IDCard")),
                            Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Phone")),
                            InsuranceNo = reader.IsDBNull(reader.GetOrdinal("InsuranceNo"))
                                          ? null
                                          : reader.GetString(reader.GetOrdinal("InsuranceNo")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried"))
                                        ? (bool?)null
                                        : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                            Nation = reader.IsDBNull(reader.GetOrdinal("Nation"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Nation")),
                            WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit"))
                                       ? null
                                       : reader.GetString(reader.GetOrdinal("WorkUnit")),
                            Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation"))
                                         ? null
                                         : reader.GetString(reader.GetOrdinal("Occupation")),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address"))
                                      ? null
                                      : reader.GetString(reader.GetOrdinal("Address")),
                            PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                            DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                            GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName"))
                                           ? null
                                           : reader.GetString(reader.GetOrdinal("GuardianName")),
                            GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship"))
                                                   ? null
                                                   : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                            GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress"))
                                              ? null
                                              : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                            GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                            Remark = reader.IsDBNull(reader.GetOrdinal("Remark"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Remark"))
                        };
                        patientList.Add(patient);

                    }

                }
            }
        }

        [HttpGet("Visit/CallNumber/{appointmentID}")]
        public async Task<IActionResult> CallNumber(int appointmentID)//控制器--医生叫号
        {
            var viewModel = new CallNumberViewModel();
            string sql = @"select a.AppointmentID,a.PatientID,p.[Name],s.ScheduleID,a.[Status],a.CreatedAt,a.Remark
,s.TimeSlot,d.DoctorID,d.DoctorName
from Appointments a 
inner join Schedules s on a.ScheduleID =s.ScheduleID
inner join Patients p on a.PatientID = p.PatientID
inner join Doctors d on d.DoctorID=s.DoctorID
where  AppointmentID =@AppointmentID";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            viewModel.AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID"));
                            viewModel.PatientID = reader.GetInt32(reader.GetOrdinal("PatientID"));
                            viewModel.PatientName = reader.GetString(reader.GetOrdinal("Name"));
                            viewModel.ScheduleID = reader.GetInt32(reader.GetOrdinal("ScheduleID"));
                            viewModel.Status = reader.GetString(reader.GetOrdinal("Status"));
                            viewModel.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                            viewModel.Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? "" : reader.GetString(reader.GetOrdinal("Remark"));
                            viewModel.TimeSlot = reader.GetString(reader.GetOrdinal("TimeSlot"));
                            viewModel.DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID"));
                            viewModel.DoctorName = reader.GetString(reader.GetOrdinal("DoctorName"));
                        }
                        else
                        {
                            // 没有找到该预约
                            return NotFound($"未找到 ID 为 {appointmentID} 的预约。");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // 记录日志（假设有 ILogger）
                // _logger.LogError(ex, "数据库查询失败");
                return StatusCode(500, "服务器内部错误，请稍后重试。");
            }

            return View(viewModel);

        }
        [HttpPost]
        public async Task<IActionResult> CallNumber(int appointmentID, string status)//控制器--提交医生叫号
        {
            status = "已叫号";
            string updateSql = @"UPDATE Appointments SET [Status] = @Status WHERE AppointmentID = @AppointmentID";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);

                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        // 没有找到对应的预约
                        ModelState.AddModelError("", "未找到指定的预约，请确认预约ID是否正确。");
                        ViewBag.AppointmentID = appointmentID;
                        // 重定向回 GET 页面，以便显示错误
                        return RedirectToAction("CallNumberIndex", new { appointmentID });
                    }
                }

                // 更新成功后，可以重定向到其他页面（如预约列表）
                TempData["SuccessMessage"] = "叫号成功！";
                return RedirectToAction("CallNumberIndex", new { appointmentID });
            }
            catch (SqlException ex)
            {
                // 记录日志（可以使用 ILogger）
                ModelState.AddModelError("", "数据库操作失败，请稍后重试。");
                // 开发环境下可以显示详细错误（可选）
                ViewBag.AppointmentID = appointmentID;
                return RedirectToAction("CallNumberIndex", new { appointmentID });
            }

        }

        public async Task<IActionResult> CallNumberIndex(int appointmentID)//控制器--确认医生叫号返回的显示页
        {
            var viewModel = new CallNumberViewModel();
            string sql = @"select a.AppointmentID,a.PatientID,p.[Name],s.ScheduleID,a.[Status],a.CreatedAt,a.Remark
,s.TimeSlot,d.DoctorID,d.DoctorName
from Appointments a 
inner join Schedules s on a.ScheduleID =s.ScheduleID
inner join Patients p on a.PatientID = p.PatientID
inner join Doctors d on d.DoctorID=s.DoctorID
where  AppointmentID =@AppointmentID";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            viewModel.AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID"));
                            viewModel.PatientID = reader.GetInt32(reader.GetOrdinal("PatientID"));
                            viewModel.PatientName = reader.GetString(reader.GetOrdinal("Name"));
                            viewModel.ScheduleID = reader.GetInt32(reader.GetOrdinal("ScheduleID"));
                            viewModel.Status = reader.GetString(reader.GetOrdinal("Status"));
                            viewModel.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                            viewModel.Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? "" : reader.GetString(reader.GetOrdinal("Remark"));
                            viewModel.TimeSlot = reader.GetString(reader.GetOrdinal("TimeSlot"));
                            viewModel.DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID"));
                            viewModel.DoctorName = reader.GetString(reader.GetOrdinal("DoctorName"));
                        }
                        else
                        {
                            // 没有找到该预约
                            return NotFound($"未找到 ID 为 {appointmentID} 的预约。");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // 记录日志（假设有 ILogger）
                // _logger.LogError(ex, "数据库查询失败");
                return StatusCode(500, "服务器内部错误，请稍后重试。");
            }

            return View(viewModel);

        }
        [HttpGet("Visit/GetPatientById/{appointmentID}")]
        public async Task<IActionResult>  GetPatientById(int appointmentID)//控制器--查看某个患者所有信息
        {
            int patientId = -1;
            var patient = new List<Patient>
{
    new Patient
    {
        PatientID = 1,
        UserId = 3,
        Name = "王五",
        Birthday = DateTime.Parse("2000-01-17 18:31:42.000"),
        Gender = "男",
        IDCard = "110101199001011234",
        Phone = "13800138002",
        InsuranceNo = "BJ123456789",
        CreatedAt = DateTime.Parse("2024-01-01 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    } };

            patientId = await GetPatientIdByAppointmentId(appointmentID);
           var isExist = await CheckPatientInfoIsExist(patientId);
            if (!isExist)
            {
                TempData["ErrorMessage"] = $"找不到该患者信息，患者id:{patientId}";
                var p = new List<Patient>();
                return View(p);
            }
            string sql = @$"SELECT  [PatientID]
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
  FROM [Patients]
  where PatientID ='{patientId}'";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var p = new Patient
                        {
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("Gender"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Gender")),
                            IDCard = reader.IsDBNull(reader.GetOrdinal("IDCard"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("IDCard")),
                            Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Phone")),
                            InsuranceNo = reader.IsDBNull(reader.GetOrdinal("InsuranceNo"))
                                          ? null
                                          : reader.GetString(reader.GetOrdinal("InsuranceNo")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried"))
                                        ? (bool?)null
                                        : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                            Nation = reader.IsDBNull(reader.GetOrdinal("Nation"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Nation")),
                            WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit"))
                                       ? null
                                       : reader.GetString(reader.GetOrdinal("WorkUnit")),
                            Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation"))
                                         ? null
                                         : reader.GetString(reader.GetOrdinal("Occupation")),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address"))
                                      ? null
                                      : reader.GetString(reader.GetOrdinal("Address")),
                            PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                            DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory"))
                                                 ? null
                                                 : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                            GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName"))
                                           ? null
                                           : reader.GetString(reader.GetOrdinal("GuardianName")),
                            GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship"))
                                                   ? null
                                                   : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                            GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress"))
                                              ? null
                                              : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                            GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                            Remark = reader.IsDBNull(reader.GetOrdinal("Remark"))
                                     ? null
                                     : reader.GetString(reader.GetOrdinal("Remark"))
                        };
                        patient.Add(p);
                    }
                }
            }
            GetPatientByIdInfo(patientId, out patient);
            ViewBag.AppointmentID = appointmentID;
            return View(patient);
        }


        [HttpGet("Visit/GetPatientMedicalRecordById/{appoinmentId}")]
        public async Task<IActionResult> GetPatientMedicalRecordById(int appoinmentId)  //控制器--查看某个患者病历记录
        {
            //这个到时重新改改设计一下
            var patientList = new List<Patient>
{
    new Patient
    {
        PatientID = 1,
        UserId = 3,
        Name = "王五",
        Birthday = DateTime.Parse("2000-01-17 18:31:42.000"),
        Gender = "男",
        IDCard = "110101199001011234",
        Phone = "13800138002",
        InsuranceNo = "BJ123456789",
        CreatedAt = DateTime.Parse("2024-01-01 00:00:00.000"),
        UpdatedAt = null,
        IsMarried = null,
        Nation = null,
        WorkUnit = null,
        Occupation = null,
        Address = null,
        PastMedicalHistory = null,
        DrugAllergyHistory = null,
        GuardianName = null,
        GuardianRelationship = null,
        GuardianAddress = null,
        GuardianPhone = null,
        Remark = null
    } };

            patientList = new List<Patient>();
            var patient = patientList.FirstOrDefault();


            #region 某个患者记录
            var medicalRecord = new List<MedicalRecord>
{
    new MedicalRecord
    {
        RecordID = 1,
        AppointmentID = 1,
        PatientStatement = "头痛、发烧三天",
        Diagnosis = "上呼吸道感染",
        Treatment = "休息、多喝水、药物治疗",
        Status = "出诊",
        CreatedAt = DateTime.Now,
        UpdatedAt = null
    }
};
            #endregion
            int patientid = -1;
            patientid = await GetPatientIdByAppointmentId(appoinmentId);
          
            var isExis =await CheckPatientInfoIsExist(patientid);
            if (!isExis)
            {
                TempData["ErrorMessage"] = $"找不到该患者信息，患者id:{patientid}";
            }
            GetPatientsInfo(out patientList);//获取所有患者信息
            GetPatientByIdInfo(patientid, out patientList);//获取某个患者信息
            patient = patientList.FirstOrDefault();
            GetMedicalRecordByPatientId(patientid, out medicalRecord);//获取某个患者病历记录

            // 创建一个包含患者姓名和医疗记录的视图模型
            var viewModel = new Tuple<List<MedicalRecord>, Patient>(medicalRecord, patient);
            return View(viewModel);

        }

        public virtual void GetMedicalRecordByPatientId(int patientId, out List<MedicalRecord> medicalRecordList)//--方法，某个患者病历记录
        {
            medicalRecordList = new List<MedicalRecord>();
            string sql = @"
        SELECT m.[RecordID], m.[AppointmentID], m.[PatientStatement], m.[Diagnosis], 
               m.[Treatment], m.[Status], m.[CreatedAt], m.[UpdatedAt]
        FROM [MedicalRecords] m
        INNER JOIN [Appointments] a ON a.AppointmentID = m.AppointmentID
        INNER JOIN [Patients] p ON p.PatientID = a.PatientID
        WHERE p.PatientID = @PatientId";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PatientId", patientId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var record = new MedicalRecord
                        {
                            RecordID = reader.GetInt32(reader.GetOrdinal("RecordID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientStatement = reader.IsDBNull(reader.GetOrdinal("PatientStatement"))
                                                ? null
                                                : reader.GetString(reader.GetOrdinal("PatientStatement")),
                            Diagnosis = reader.IsDBNull(reader.GetOrdinal("Diagnosis"))
                                         ? null
                                         : reader.GetString(reader.GetOrdinal("Diagnosis")),
                            Treatment = reader.IsDBNull(reader.GetOrdinal("Treatment"))
                                         ? null
                                         : reader.GetString(reader.GetOrdinal("Treatment")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                                      ? null
                                      : reader.GetString(reader.GetOrdinal("Status")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                         ? (DateTime?)null
                                         : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        };
                        medicalRecordList.Add(record);
                    }
                }
            }
        }
        public virtual async Task<bool> CheckPatientInfoIsExist(int patientId)//方法，检查某个患者信息存不存在，会影响病历记录显示等
        {
            if (patientId <= 0) {
                throw new ArgumentException("患者ID无效，不能为负数。", nameof(patientId));
            }
            string sql = @"select  COUNT(1) from Patients where PatientID =@PatientID";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PatientID", patientId);
                    await conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }

            }
            catch (Exception ex)
            {
                // 记录日志（可以使用 ILogger）
                // _logger.LogError(ex, "查询病历是否存在时发生错误，AppointmentID: {AppointmentID}", appointmentID);
                throw ex;
            }
        }
        

        public async Task<IActionResult> CreateMedicalRecord(int id)//控制器--问诊、录入病历CreateMedicalRecord
        {

            int appointmentId = id;
            var medicalRecord = new MedicalRecord
            {
                AppointmentID = appointmentId
            };
            if (id == 0)
            {
                TempData["ErrorMessage"] = ("挂号ID无效，不能为0！");
            }
            int patientId = await GetPatientIdByAppointmentId(appointmentId);
            var isExits = await CheckPatientInfoIsExist(patientId);
            if (!isExits)
            {
                TempData["ErrorMessage"] = "没有找到该患者个人信息！";
                return View(medicalRecord);
            }
            var isExist = await CheckedMedicalRecordIfExistAsync(appointmentId);
            if (isExist)
            {
                TempData["ErrorMessage"] = $"该挂号id{appointmentId}：病历已存在，不能插入！";
                return RedirectToAction("CreateMedicalRecord", new { appointmentId });
            }



            return View(medicalRecord);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMedicalRecord(int appointmentId, MedicalRecord createMedicalRecord)//控制器-录入病历
        {

            try
            {
                var isExist = await CheckedMedicalRecordIfExistAsync(appointmentId);
                if (isExist)
                {
                    TempData["ErrorMessage"] = $"该挂号id{appointmentId}：病历已存在，不能插入！";
                    return RedirectToAction("CreateMedicalRecord", new { appointmentId });
                }
                await InsertMedicalRecordAsync(createMedicalRecord);
                TempData["SuccessMessage"] = "录入病历成功！";
                return RedirectToAction("GetPatientMedicalRecordById", new { id=appointmentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"录入病历失败：{ex.Message}";
                return RedirectToAction("GetPatientMedicalRecordById", new { id=appointmentId });
            }
        }
        #region 查询MedicalRecord存不存在
        public virtual async Task<bool> CheckedMedicalRecordIfExistAsync(int id)//方法
        {
            int appointmentID = id;
            string sql = "SELECT COUNT(1) FROM MedicalRecords WHERE AppointmentID = @AppointmentID";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                    await conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                // 记录日志（可以使用 ILogger）
                // _logger.LogError(ex, "查询病历是否存在时发生错误，AppointmentID: {AppointmentID}", appointmentID);
                throw ex; // 重新抛出，保留原始堆栈
            }
        }

        #endregion
        #region 插入MedicalRecords方法  
        public virtual async Task InsertMedicalRecordAsync(MedicalRecord medicalRecord)//方法
        {

            string InsertSql = @"INSERT INTO dbo.MedicalRecords (AppointmentID, PatientStatement, Diagnosis, Treatment, [Status], CreatedAt, UpdatedAt)  
                               VALUES   
                               (@AppointmentID, @PatientStatement, @Diagnosis, @Treatment, @Status, @CreatedAt, @UpdatedAt)";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(InsertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", medicalRecord.AppointmentID);
                    cmd.Parameters.AddWithValue("@PatientStatement", medicalRecord.PatientStatement ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Diagnosis", medicalRecord.Diagnosis ?? "");
                    cmd.Parameters.AddWithValue("@Treatment", medicalRecord.Treatment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", medicalRecord.Status ?? "");
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UpdatedAt", medicalRecord.UpdatedAt ?? (object)DBNull.Value);

                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("插入病历记录失败，未影响任何行。");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("插入病历记录时发生错误。", ex);
            }
        }
        #endregion

        #region 查询患者id用AppointId
        public virtual async Task<int> GetPatientIdByAppointmentId(int appointmentID)//方法
        {
            int patientId = 0;
            string sql = @"select PatientID from Appointments where AppointmentID =@AppointmentID";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                    await conn.OpenAsync();
                    object result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        patientId = Convert.ToInt32(result);
                        return patientId; // 成功找到记录
                    }
                    else
                    {
                        throw new Exception($"未找到{appointmentID}的患者");
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取患者ID失败，预约ID: {AppointmentID}", appointmentID);
                // 可以选择重新抛出或返回错误码
                throw; // 向上层抛出异常，由调用方处理
            }

        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> EditPatientMedicalRecordById0(int id)//不用的控制器 -修改患者病历记录 id 为 AppointmentID
        {
            int appointmentId = id;

            var patientList = new List<Patient>();
            int patientid = await GetPatientIdByAppointmentId(appointmentId);

            #region 某个患者记录
            var medicalRecord = new List<MedicalRecord>();
            #endregion
            GetPatientsInfo(out patientList);//获取所有患者信息
            GetPatientByIdInfo(patientid, out patientList);//获取某个患者信息
            var patient = patientList.FirstOrDefault();
            GetMedicalRecordByPatientId(patientid, out medicalRecord);//获取某个患者病历记录

            // 创建一个包含患者姓名和医疗记录的视图模型
            var viewModel = new Tuple<List<MedicalRecord>, Patient>(medicalRecord, patient);

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> EditPatientMedicalRecordById(int id)  //控制器-修改患者病历记录 id 为 AppointmentID
        {
            
            // 1. 根据 AppointmentID 获取病历记录
            var medicalRecord = await GetMedicalRecordByAppointmentIdAsync(id);

            if (medicalRecord == null)
            {
                // 如果没有病历记录，可以跳转到创建页面
                TempData["ErrorMessage"] = "该预约暂无病历记录，请先创建。";
                return RedirectToAction("CreateMedicalRecord", new { id = id });
            }

            // 2. 获取患者信息（用于显示姓名）
            int patientId = await GetPatientIdByAppointmentId(id);
            var patient = await GetPatientByIdAsync(patientId);
            ViewBag.PatientName = patient?.Name ?? "未知患者";

            // 3. 返回编辑视图
            return View(medicalRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatientMedicalRecordById(MedicalRecord model)//控制器-修改患者病历记录提交
        {
            if (!ModelState.IsValid)
            {
                int appointmentId = model.AppointmentID;
                // 模型验证失败，重新加载患者信息并返回视图
                int patientId = await GetPatientIdByAppointmentId(appointmentId);
                var isExits = await  CheckPatientInfoIsExist(patientId);
                if (!isExits)
                {
                    TempData["ErrorMessage"] = "没有找到该患者个人信息！";
                    return View(model);
                }
                var patient = await GetPatientByIdAsync(patientId);
                ViewBag.PatientName = patient?.Name ?? "未知患者";
                return View(model);
            }

            string updateSql = @"
        UPDATE MedicalRecords 
        SET 
            PatientStatement = @PatientStatement,
            Diagnosis = @Diagnosis,
            Treatment = @Treatment,
            [Status] = @Status,
            UpdatedAt = @UpdatedAt
        WHERE AppointmentID = @AppointmentID";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@PatientStatement", model.PatientStatement ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Diagnosis", model.Diagnosis ?? "");
                cmd.Parameters.AddWithValue("@Treatment", model.Treatment ?? "");
                cmd.Parameters.AddWithValue("@Status", model.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@AppointmentID", model.AppointmentID);

                try
                {
                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        // 未找到对应记录
                        TempData["ErrorMessage"] = "未找到该病历记录，可能已被删除。";
                        return RedirectToAction("GetPatientMedicalRecordById", new { id = model.AppointmentID });
                    }

                    TempData["SuccessMessage"] = "病历记录更新成功！";
                    return RedirectToAction("GetPatientMedicalRecordById", new { id = model.AppointmentID });
                }
                catch (SqlException ex)
                {
                    // 记录日志（建议注入 ILogger）
                    TempData["ErrorMessage"] = "数据库操作失败，请稍后重试。";
                    // 开发时可记录详细错误：_logger.LogError(ex, "更新病历记录失败");
                    // 重新加载患者信息并返回视图
                    int patientId = await GetPatientIdByAppointmentId(model.AppointmentID);
                    var patient = await GetPatientByIdAsync(patientId);
                    ViewBag.PatientName = patient?.Name ?? "未知患者";
                    return View(model);
                }
            }
        }
        #region 根据患者ID获取患者信息
        public virtual async Task<Patient> GetPatientByIdAsync(int patientId)//方法-根据患者ID获取患者信息
        {
            var patient = new Patient();
            string sql = @"
                       select  
                       p.[PatientID]      ,[UserId]      ,[Name]      ,[Birthday]      ,[Gender]      ,[IDCard]
                     ,[Phone]      ,[InsuranceNo]      ,p.[CreatedAt]      ,[UpdatedAt]      ,[IsMarried]
                     ,[Nation]      ,[WorkUnit]      ,[Occupation]      ,[Address]      ,[PastMedicalHistory]
                     ,[DrugAllergyHistory]      ,[GuardianName]      ,[GuardianRelationship]      ,[GuardianAddress]
                     ,[GuardianPhone]      ,p.[Remark]
	                 from Patients p
                       inner join  Appointments a on p.PatientID =a.PatientID
                       where p.PatientID =@PatientID";
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
                            // 创建并填充 Patient 对象
                             patient = new Patient
                            {
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                                Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                                Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? string.Empty : reader.GetString(reader.GetOrdinal("Gender")),
                                IDCard = reader.IsDBNull(reader.GetOrdinal("IDCard")) ? string.Empty : reader.GetString(reader.GetOrdinal("IDCard")),
                                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? string.Empty : reader.GetString(reader.GetOrdinal("Phone")),
                                InsuranceNo = reader.IsDBNull(reader.GetOrdinal("InsuranceNo")) ? string.Empty : reader.GetString(reader.GetOrdinal("InsuranceNo")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                IsMarried = reader.IsDBNull(reader.GetOrdinal("IsMarried")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("IsMarried")),
                                Nation = reader.IsDBNull(reader.GetOrdinal("Nation")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nation")),
                                WorkUnit = reader.IsDBNull(reader.GetOrdinal("WorkUnit")) ? string.Empty : reader.GetString(reader.GetOrdinal("WorkUnit")),
                                Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? string.Empty : reader.GetString(reader.GetOrdinal("Occupation")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? string.Empty : reader.GetString(reader.GetOrdinal("Address")),
                                PastMedicalHistory = reader.IsDBNull(reader.GetOrdinal("PastMedicalHistory")) ? string.Empty : reader.GetString(reader.GetOrdinal("PastMedicalHistory")),
                                DrugAllergyHistory = reader.IsDBNull(reader.GetOrdinal("DrugAllergyHistory")) ? string.Empty : reader.GetString(reader.GetOrdinal("DrugAllergyHistory")),
                                GuardianName = reader.IsDBNull(reader.GetOrdinal("GuardianName")) ? string.Empty : reader.GetString(reader.GetOrdinal("GuardianName")),
                                GuardianRelationship = reader.IsDBNull(reader.GetOrdinal("GuardianRelationship")) ? string.Empty : reader.GetString(reader.GetOrdinal("GuardianRelationship")),
                                GuardianAddress = reader.IsDBNull(reader.GetOrdinal("GuardianAddress")) ? string.Empty : reader.GetString(reader.GetOrdinal("GuardianAddress")),
                                GuardianPhone = reader.IsDBNull(reader.GetOrdinal("GuardianPhone")) ? string.Empty : reader.GetString(reader.GetOrdinal("GuardianPhone")),
                                Remark = reader.IsDBNull(reader.GetOrdinal("Remark")) ? string.Empty : reader.GetString(reader.GetOrdinal("Remark"))
                            };
                            return patient;
                    }
                    else
                    {
                        throw new Exception($"未找到id为{patientId}的患者");
                   
                    }
                }
            }
            }
            catch (Exception ex )
            {

                throw ex;
            }

        }
        #endregion

        #region//根据AppointmentID获取病历记录
        private async Task<MedicalRecord> GetMedicalRecordByAppointmentIdAsync(int id)//方法-根据AppointmentID获取病历记录
        {
            int appointmentId = id;
            var  medicalRecord = new MedicalRecord();
           string sql = @"
            select RecordID,AppointmentID,PatientStatement,Diagnosis,Treatment,[Status],	CreatedAt	,UpdatedAt
            from MedicalRecords where AppointmentID =@AppointmentID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                         medicalRecord = new MedicalRecord
                        {
                            RecordID = reader.GetInt32(reader.GetOrdinal("RecordID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientStatement = reader.GetString(reader.GetOrdinal("PatientStatement")),
                            Diagnosis = reader.GetString(reader.GetOrdinal("Diagnosis")),
                            Treatment = reader.IsDBNull(reader.GetOrdinal("Treatment"))?null:reader.GetString(reader.GetOrdinal("Treatment")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        };
                        return medicalRecord;
                    }
                    else
                    {
                        return null;//没有找到记录
                        //throw new Exception($"没有找到{appointmentId}的：病历历史记录");
                    }
                }
            }



        }
        #endregion


        public IActionResult CreateCheckOrder(int id)//控制器//开检查单
        {
            int appointmentId = id;
            //这个也要好好设计
            _checkItems = new List<CheckItem>();
            GetCheckItemInfo(out _checkItems);
            var checkItems = _checkItems
                .Where(ci => ci.IsActive)
                .Select(ci => new
                {
                    CheckItemID = ci.CheckItemID,
                    DisplayText = ci.Name + (string.IsNullOrEmpty(ci.Description) ? "" : " - " + ci.Description)
                })
                .ToList();

            ViewBag.CheckItemID = new SelectList(checkItems, "CheckItemID", "DisplayText");

            // 预约下拉列表：使用 AppointmentID 作为值和显示文本（可自定义显示格式）
            _appointmentIds = new List<int>();
            GetAppointmentIds(out _appointmentIds);

            var appointments = _appointmentIds.Select(id => new { AppointmentID = id, DisplayText = $"挂号#{id}" }).ToList();
            ViewBag.AppointmentID = new SelectList(appointments, "AppointmentID", "DisplayText");

            return View();
        }

        #region 备份
        public IActionResult CreateCheckOrderbak()//备份控制器//开检查单
        {
            //这个也要好好设计
            _checkItems = new List<CheckItem>();
            GetCheckItemInfo(out _checkItems);
            var checkItems = _checkItems
                .Where(ci => ci.IsActive)
                .Select(ci => new
                {
                    CheckItemID = ci.CheckItemID,
                    DisplayText = ci.Name + (string.IsNullOrEmpty(ci.Description) ? "" : " - " + ci.Description)
                })
                .ToList();

            ViewBag.CheckItemID = new SelectList(checkItems, "CheckItemID", "DisplayText");

            // 预约下拉列表：使用 AppointmentID 作为值和显示文本（可自定义显示格式）
            _appointmentIds = new List<int>();
            GetAppointmentIds(out _appointmentIds);

            var appointments = _appointmentIds.Select(id => new { AppointmentID = id, DisplayText = $"挂号#{id}" }).ToList();
            ViewBag.AppointmentID = new SelectList(appointments, "AppointmentID", "DisplayText");

            return View();
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckOrder checkOrder)//控制器-创建检查单提交
        {
          
            if (ModelState.IsValid)
            {
                // 模拟保存操作（实际应存入数据库）
                checkOrder.CreatedAt = DateTime.Now;
                checkOrder.Status = "已开单"; // 默认状态

                // 这里可以模拟保存到列表，例如：
                // _checkOrders.Add(checkOrder); （需定义静态列表）

                TempData["SuccessMessage"] = "检查单创建成功！";
                return RedirectToAction(nameof(Index)); // 假设存在 Index 动作
            }

            _checkItems = new List<CheckItem>();
            GetCheckItemInfo(out _checkItems);
            // 验证失败，重新填充下拉列表
            var checkItems = _checkItems
                .Where(ci => ci.IsActive)
                .Select(ci => new
                {
                    CheckItemID = ci.CheckItemID,
                    DisplayText = ci.Name + (string.IsNullOrEmpty(ci.Description) ? "" : " - " + ci.Description)
                })
                .ToList();
            ViewBag.CheckItemID = new SelectList(checkItems, "CheckItemID", "DisplayText", checkOrder.CheckItemID);

            var appointments = _appointmentIds.Select(id => new { AppointmentID = id, DisplayText = $"预约 #{id}" }).ToList();
            ViewBag.AppointmentID = new SelectList(appointments, "AppointmentID", "DisplayText", checkOrder.AppointmentID);

            return View(checkOrder);
            
        }

        #region CheckOrdersIndexbak
        // GET: CheckOrders/Index
        public IActionResult CheckOrdersIndexbak()//控制器-患者检查单列表展示
        {
            #region// 模拟检查单数据（基于您提供的示例）
            var checkOrders = new List<CheckOrder>
    {
        new CheckOrder { CheckOrderID = 1, AppointmentID = 1, CheckItemID = 1, Status = "已开单", Result = "血脂偏高", CreatedAt = new DateTime(2024, 1, 15), UpdatedAt = null },
        new CheckOrder { CheckOrderID = 2, AppointmentID = 2, CheckItemID = 2, Status = "已检查", Result = "检查结果正常", CreatedAt = new DateTime(2024, 1, 15), UpdatedAt = new DateTime(2024, 1, 15) },
        new CheckOrder { CheckOrderID = 3, AppointmentID = 3, CheckItemID = 3, Status = "已出报告", Result = "心电图正常", CreatedAt = new DateTime(2024, 1, 16), UpdatedAt = new DateTime(2024, 1, 16) },
        new CheckOrder { CheckOrderID = 4, AppointmentID = 4, CheckItemID = 4, Status = "已审核", Result = "B超显示正常", CreatedAt = new DateTime(2024, 1, 16), UpdatedAt = new DateTime(2024, 1, 17) },
        new CheckOrder { CheckOrderID = 5, AppointmentID = 5, CheckItemID = 5, Status = "已检查", Result = "CT扫描正常", CreatedAt = new DateTime(2024, 1, 20), UpdatedAt = new DateTime(2024, 1, 20) },
        new CheckOrder { CheckOrderID = 6, AppointmentID = 6, CheckItemID = 6, Status = "已开单", Result = "X光片显示正常", CreatedAt = new DateTime(2024, 1, 17), UpdatedAt = null },
        new CheckOrder { CheckOrderID = 7, AppointmentID = 7, CheckItemID = 6, Status = "已检查", Result = "X光片显示正常", CreatedAt = new DateTime(2024, 1, 18), UpdatedAt = new DateTime(2024, 1, 18) }
    };

            var patients = new List<Patient>
    {
        new Patient { PatientID = 1, Name = "王五" },
        new Patient { PatientID = 2, Name = "赵六" },
        new Patient { PatientID = 3, Name = "钱七" },
        new Patient { PatientID = 4, Name = "孙八" },
        new Patient { PatientID = 5, Name = "吴十" },
        new Patient { PatientID = 6, Name = "郑十一" },
        new Patient { PatientID = 7, Name = "王十二" },
        new Patient { PatientID = 13, Name = "2" }  // 第13条记录，姓名为"2"（可能是测试数据）
    };
            #endregion
            checkOrders = new List<CheckOrder>();
            // 构建 AppointmentID 到患者姓名的字典（假设 AppointmentID == PatientID）
            var patientDict = patients.ToDictionary(p => p.PatientID, p => p.Name);
            // 检查项目数据（复用之前的 _checkItems）
            // 为了在视图中方便获取项目名称，可以将检查项目列表存入 ViewBag 或使用 Join
            ViewBag.CheckItems = _checkItems.ToDictionary(ci => ci.CheckItemID, ci => ci.Name);
            
            return View(checkOrders);
        }
        #endregion
        #endregion
        #region 备份方法
        public virtual async Task<List<CheckOrder>> GetCheckOrderList()//备份方法，获取检查单列表
        {
            var checkOrdersList = new List<CheckOrder>();
            string sql = @"	
                           select[CheckOrderID]
                         ,[AppointmentID]
                         ,[CheckItemID]
                         ,[Status]
                         ,[Result]
                         ,[CreatedAt]
                         ,[UpdatedAt]
	                     from CheckOrders";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var order = new CheckOrder
                                {
                                    CheckOrderID = reader.GetInt32(reader.GetOrdinal("CheckOrderID")),
                                    AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                    CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString(reader.GetOrdinal("Result")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                                };
                                checkOrdersList.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取检查单列表失败");
                // 根据需要可以选择重新抛出或返回空列表
                return null;
            }
            return checkOrdersList;
        }
        #endregion


        public virtual async Task<List<ChekItemNameList>> GetChekItemNameByAppointmentId(int id) //方法，还没用显示checkItem名
        {
            var chekItemNameList = new List<ChekItemNameList>();
            string sql = @"
            select c.AppointmentID,c.CheckOrderID,ci.CheckItemID,ci.Name from CheckItems ci
	        inner join CheckOrders c 	  on ci.CheckItemID =c.CheckItemID
	        inner join Appointments a   on a.AppointmentID =c.AppointmentID";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        //cmd.Parameters.AddWithValue("@AppointmentID");//通过AppointmentID id查询才需要这句
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var chekItemName = new ChekItemNameList
                                {
                                    AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),

                                    CheckOrderID = reader.GetInt32(reader.GetOrdinal("CheckOrderID")),
                                    CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                };
                                chekItemNameList.Add(chekItemName);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取检查单列表失败");
                // 根据需要可以选择重新抛出或返回空列表
                return null;
            }
            return chekItemNameList;
        }

        #region 分界线

        #endregion


        /// <summary>
        /// 控制器-患者检查单列表展示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CheckOrdersIndex()//控制器-患者检查单列表展示
        {
            var checkOrders =new List<CheckOrdersIndex>();
            checkOrders =await GetCheckOrdersIndexList();
            return View(checkOrders);
        }
        public virtual async Task<List<CheckOrdersIndex>> GetCheckOrdersIndexList()//方法，获取检查单列表展示用
        {
            var checkOrdersIndexList = new List<CheckOrdersIndex>();
            string sql = @"	
	                        select
	                    	p.PatientID,
	                    	p.[Name] as PatientName,
		                    ci.Name	as CheckItemName,
	                       co.[CheckOrderID]
                          ,co.[AppointmentID]
                          ,co.[CheckItemID]
                          ,co.[Status]
                          ,co.[Result]
                          ,co.[CreatedAt]
                          ,co.[UpdatedAt]
	                      from CheckOrders co
	                      inner join Appointments a on a.AppointmentID =co.AppointmentID
	                      inner join Patients p on p.PatientID =a.PatientID
                          inner join CheckItems ci on  ci.CheckItemID =co.CheckItemID
	                      --where co.AppointmentID =@AppointmentID --通过AppointmentID id查询才需要这句
";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        //cmd.Parameters.AddWithValue("@AppointmentID");//通过AppointmentID id查询才需要这句
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var checkOrdersIndex = new CheckOrdersIndex
                                {
                                    PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                    PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                    CheckItemName = reader.GetString(reader.GetOrdinal("CheckItemName")),
                                    CheckOrderID = reader.GetInt32(reader.GetOrdinal("CheckOrderID")),
                                    AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                    CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString(reader.GetOrdinal("Result")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                                };
                                checkOrdersIndexList.Add(checkOrdersIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取检查单列表失败");
                // 根据需要可以选择重新抛出或返回空列表
                return null;
            }
            return checkOrdersIndexList;
        }

        /// <summary>
        /// 控制器-创建检查单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateCheckOrders(int id)//控制器-创建检查单
        {
            int appointmentId = id;
            // 获取检查项目列表
            var checkItems = await GetCheckItemNameList();
            if (checkItems == null) checkItems = new List<CheckItemNames>();
            ViewBag.CheckItems = new SelectList(checkItems, "CheckItemID", "CheckItemName");
            var checkOrders = new CheckOrder
            {
                AppointmentID = appointmentId,
                Status = "已开单"  // 默认状态
            };

            return View(checkOrders);
        }
        /// <summary>
        /// 控制器，提交创建检查单
        /// </summary>
        /// <param name="checkOrder"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCheckOrders(CheckOrder checkOrder)//控制器-提交创建检查单
        {

            if (!ModelState.IsValid)
            {
                // 重新加载检查项目列表
                var checkItems = await GetCheckItemNameList();
                ViewBag.CheckItems = new SelectList(checkItems, "CheckItemID", "CheckItemName");
                return View(checkOrder);
            }

            // 设置创建时间等默认值
            checkOrder.CreatedAt = DateTime.Now;
            checkOrder.UpdatedAt = null;

            // 调用插入数据库的方法（假设存在 InsertCheckOrderAsync）
            int newId =  await InsertCheckOrderAsync(checkOrder);
            if (newId>0)
            {
                TempData["SuccessMessage"] = "检查单创建成功！";
                return RedirectToAction("CheckOrdersIndex");
            }
            else
            {
              TempData["ErrorMessage"] = "创建失败，请稍后重试。";
              return View(checkOrder);
            }
           
        }

        public virtual async Task<int> InsertCheckOrderAsync(CheckOrder checkOrder)
        {
            string insertSql = @"INSERT INTO dbo.CheckOrders (AppointmentID, CheckItemID, [Status], Result, CreatedAt, UpdatedAt)
                                OUTPUT INSERTED.CheckOrderID
                                VALUES (@AppointmentID, @CheckItemID, @Status, @Result, @CreatedAt, @UpdatedAt)";
            try
            {


            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", checkOrder.AppointmentID);
                cmd.Parameters.AddWithValue("@CheckItemID", checkOrder.CheckItemID);
                cmd.Parameters.AddWithValue("@Status", checkOrder.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Result", checkOrder.Result ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", checkOrder.CreatedAt);
                cmd.Parameters.AddWithValue("@UpdatedAt", checkOrder.UpdatedAt ?? (object)DBNull.Value);

                await conn.OpenAsync();
                    // ExecuteScalarAsync 返回第一行第一列，即新生成的 ID
                    object result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    // 理论上 INSERT 应该成功，但如果未能获取到 ID，可以抛出异常或返回 0
                    throw new InvalidOperationException("未能获取新插入的 CheckOrderID。");
                }
            }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //方法
        public async Task<List<CheckItemNames>> GetCheckItemNameList()//方法--获取检查项目名称列表
        {
            var checkItemNamesLit = new List<CheckItemNames>();
            string sql = @"select CheckItemID,Name as CheckItemName from CheckItems where IsActive =1 ";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var checkItemName = new CheckItemNames
                                { 
                                    CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                                    CheckItemName = reader.GetString(reader.GetOrdinal("CheckItemName"))
                    
                                };
                                checkItemNamesLit.Add(checkItemName);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取检查单列表失败");
                // 根据需要可以选择重新抛出或返回空列表
                return null;
            }

            return checkItemNamesLit;
        }

        public virtual async Task<List<CheckOrdersIndex>> GetCheckOrdersByAppointmentIdInfo(int appointmentID)//方法，获取某一行检查单列表
        {
            var checkOrdersIndexList = new List<CheckOrdersIndex>();
            string sql = @"	
	                        select
	                    	p.PatientID,
	                    	p.[Name] as PatientName,
		                    ci.Name	as CheckItemName,
	                       co.[CheckOrderID]
                          ,co.[AppointmentID]
                          ,co.[CheckItemID]
                          ,co.[Status]
                          ,co.[Result]
                          ,co.[CreatedAt]
                          ,co.[UpdatedAt]
	                      from CheckOrders co
	                      inner join Appointments a on a.AppointmentID =co.AppointmentID
	                      inner join Patients p on p.PatientID =a.PatientID
                          inner join CheckItems ci on  ci.CheckItemID =co.CheckItemID
	                      where co.AppointmentID =@AppointmentID 
";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID",appointmentID);//通过AppointmentID id查询才需要这句
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var checkOrdersIndex = new CheckOrdersIndex
                                {
                                    PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                    PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                    CheckItemName = reader.GetString(reader.GetOrdinal("CheckItemName")),
                                    CheckOrderID = reader.GetInt32(reader.GetOrdinal("CheckOrderID")),
                                    AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                    CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString(reader.GetOrdinal("Result")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                                };
                                checkOrdersIndexList.Add(checkOrdersIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取检查单列表失败");
                // 根据需要可以选择重新抛出或返回空列表
                return null;
            }
            return checkOrdersIndexList;
        }


        public virtual async Task<List<CheckOrdersIndex>> GetCheckOrdersByCheckOrderIdInfo(int checkOrderID)//方法，通过CheckOrderID获取某一行检查单列表
        {
            var checkOrdersIndexList = new List<CheckOrdersIndex>();
            string sql = @"	
	                        select
	                    	p.PatientID,
	                    	p.[Name] as PatientName,
		                    ci.Name	as CheckItemName,
	                       co.[CheckOrderID]
                          ,co.[AppointmentID]
                          ,co.[CheckItemID]
                          ,co.[Status]
                          ,co.[Result]
                          ,co.[CreatedAt]
                          ,co.[UpdatedAt]
	                      from CheckOrders co
	                      inner join Appointments a on a.AppointmentID =co.AppointmentID
	                      inner join Patients p on p.PatientID =a.PatientID
                          inner join CheckItems ci on  ci.CheckItemID =co.CheckItemID
	                      where co.CheckOrderID =@CheckOrderID 
";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CheckOrderID", checkOrderID);//通过CheckOrderID id查询才需要这句
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var checkOrdersIndex = new CheckOrdersIndex
                                {
                                    PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                    PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                    CheckItemName = reader.GetString(reader.GetOrdinal("CheckItemName")),
                                    CheckOrderID = reader.GetInt32(reader.GetOrdinal("CheckOrderID")),
                                    AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                    CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString(reader.GetOrdinal("Result")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                                };
                                checkOrdersIndexList.Add(checkOrdersIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "获取检查单列表失败");
                // 根据需要可以选择重新抛出或返回空列表
                return null;
            }
            return checkOrdersIndexList;
        }

        //用这个
        public virtual async Task<CheckOrder> GetCheckOrderByIdAsync(int checkOrderId)//方法，只获取CheckOrder通过orderId
        {
            string sql = @"
        SELECT CheckOrderID, AppointmentID, CheckItemID, [Status], Result, CreatedAt, UpdatedAt
        FROM CheckOrders
        WHERE CheckOrderID = @CheckOrderID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CheckOrderID", checkOrderId);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new CheckOrder
                        {
                            CheckOrderID = reader.GetInt32(reader.GetOrdinal("CheckOrderID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            CheckItemID = reader.GetInt32(reader.GetOrdinal("CheckItemID")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString(reader.GetOrdinal("Result")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        };
                    }
                    else
                    {
                        return null; // 未找到记录
                    }
                }
            }
        }
        /// <summary>
        /// 控制器获取某一行检查单列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCheckOrdersByAppointmentId(int id) //控制器获取某一行检查单列表
        {
            int appointmentId = id;
            var checkOrders = new List<CheckOrdersIndex>();
            checkOrders = await GetCheckOrdersByAppointmentIdInfo(appointmentId);
            return View(checkOrders);
   
        }

        [HttpGet]
        public async Task<IActionResult> EditCheckOrdersByCheckOrderId(int id) //控制器修改某一行检查单列表
        {
            //int appointmentId = id;
            int checkOrderId = id;
            // 根据 CheckOrderID 获取检查单实体
            var checkOrder = await GetCheckOrderByIdAsync(id);
            if (checkOrder == null)
            {
                return NotFound();
            }

            // 获取检查项目列表用于下拉框
            var checkItems = await GetCheckItemNameList();
            ViewBag.CheckItems = new SelectList(checkItems, "CheckItemID", "CheckItemName", checkOrder.CheckItemID);

            return View(checkOrder);
         
        }

        /// <summary>
        ///控制器，提交修改Order订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCheckOrdersByCheckOrderId(CheckOrder model)// 控制器，提交修改Order订单
        {
            if (!ModelState.IsValid)
            {
                // 验证失败，重新加载下拉列表并返回视图
                var checkItems = await GetCheckItemNameList();
                ViewBag.CheckItems = new SelectList(checkItems, "CheckItemID", "CheckItemName", model.CheckItemID);
                return View(model);
            }

            // 调用更新方法
            bool success = await UpdateCheckOrderAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "检查单更新成功！";
                return RedirectToAction(nameof(CheckOrdersIndex)); // 重定向到列表页
            }
            else
            {
                TempData["ErrorMessage"] = "更新失败，请稍后重试。";
                // 重新加载下拉列表
                var checkItems = await GetCheckItemNameList();
                ViewBag.CheckItems = new SelectList(checkItems, "CheckItemID", "CheckItemName", model.CheckItemID);
                return View(model);
            }
        }
        public virtual async Task<List<CheckOrdersIndex>> GetEditCheckOrdersByAppointmentId(int appointmentId)//方法，获取某一行检查单列表用于编辑
        {
            var checkOrders = new List<CheckOrdersIndex>();
            checkOrders = await GetCheckOrdersByAppointmentIdInfo(appointmentId);
            return checkOrders;
        }
        public virtual async Task<bool> UpdateCheckOrderAsync(CheckOrder checkOrder)//方法，更新检查单信息
        {
            string updateSql = @"UPDATE dbo.CheckOrders
                                SET CheckItemID = @CheckItemID,
                                    [Status] = @Status,
                                    Result = @Result,
                                    UpdatedAt = @UpdatedAt
                                WHERE CheckOrderID = @CheckOrderID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@CheckItemID", checkOrder.CheckItemID);
                    cmd.Parameters.AddWithValue("@Status", checkOrder.Status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Result", checkOrder.Result ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CheckOrderID", checkOrder.CheckOrderID);
                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // 返回是否成功更新
                }
            }
            catch (Exception ex)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "更新检查单失败");
                // 根据需要可以选择重新抛出或返回 false
                return false;
            }
        }
        public async Task<IActionResult> GetMyMedicalRecord(int patientId) //控制器-获取患者病历
        {
           
            var myMedicalRecord = new List<GetMyMedicalRecordModel>();
            myMedicalRecord = await GetMyMedicalRecordInfo(patientId);
            if (myMedicalRecord == null)
            {
                return NotFound();
            }
            return View(myMedicalRecord);
        }
        public virtual async Task<List<GetMyMedicalRecordModel>> GetMyMedicalRecordInfo(int patientId)
        {
            var myMedicalRecords = new List<GetMyMedicalRecordModel>();
            string sql = @"
                        SELECT m.[RecordID], m.[AppointmentID],p.Name as PatientName, m.[PatientStatement], m.[Diagnosis], 
                        m.[Treatment], m.[Status], m.[CreatedAt], m.[UpdatedAt]
                        FROM [MedicalRecords] m
                        INNER JOIN [Appointments] a ON a.AppointmentID = m.AppointmentID
                        INNER JOIN [Patients] p ON p.PatientID = a.PatientID
                        WHERE p.PatientID = @PatientID
                        order by m.[RecordID] desc
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
                            var record = new GetMyMedicalRecordModel
                            {
                               RecordID = reader.GetInt32(reader.GetOrdinal("RecordID")),
                               AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                               PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                               PatientStatement = reader.IsDBNull(reader.GetOrdinal("PatientStatement")) ? null : reader.GetString(reader.GetOrdinal("PatientStatement")),
                               Diagnosis = reader.IsDBNull(reader.GetOrdinal("Diagnosis")) ? null : reader.GetString(reader.GetOrdinal("Diagnosis")),
                               Treatment = reader.IsDBNull(reader.GetOrdinal("Treatment")) ? null : reader.GetString(reader.GetOrdinal("Treatment")),
                               Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                               CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                               UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            };
                            myMedicalRecords.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 可根据需要记录日志
                // _logger.LogError(ex, "获取患者病历失败，PatientId: {PatientId}", patientId);
                return null; // 或者重新抛出
            }
            return myMedicalRecords;

        }

        }
}
