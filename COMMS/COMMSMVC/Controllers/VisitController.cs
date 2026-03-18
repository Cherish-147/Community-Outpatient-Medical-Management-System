using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class VisitController : Controller
    {
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        // 模拟数据
        private   List<CheckItem> _checkItems = new List<CheckItem>
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
        public virtual void GetCheckItemInfo(out List<CheckItem> checkItems)//获取所有检查单清单
        {
            checkItems =new List<CheckItem>();
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
        // 模拟预约数据（从您提供的 CheckOrders 表中提取 AppointmentID）
        private List<int> _appointmentIds = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

        public virtual void GetAppointmentIds(out List<int> appointmentIds)
        {
            appointmentIds = [];
            var appointments =new List<AppointmentViewModel>();
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
        public virtual void GetTodayPatientAppointmentInfo(out List<AppointmentViewModel> appointments)
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
        public IActionResult GetTodayPatientAppointment()
        {
          var appointments = new List<AppointmentViewModel>();
          GetTodayPatientAppointmentInfo(out appointments);

            return View(appointments);
        }

        public IActionResult GetOnedayPatientAppointment(DateTime? startDate, DateTime? endDate, out List<AppointmentViewModel> appointments)
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
        
        public virtual void GetPatientAppointmentInfo(out List<AppointmentViewModel> appointments)//查看全部患者挂号
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
        //查看全部患者挂号
        public IActionResult GetPatientAppointment()
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
        //查看患者所有信息
        public IActionResult GetPatients()
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
        public virtual void GetPatientsInfo(out List<Patient> patients )
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

        
        public virtual void GetPatientByIdInfo(int patientid, out List<Patient> patientList)//获取某个患者信息
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
                        patientList.Add(patient);

                    }

                }
            }
        }
        //查看某个患者所有信息
        public IActionResult GetPatientById(int patientId)
        {
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
            return View(patient);
        }
        //查看某个患者病历记录
        public IActionResult GetPatientMedicalRecordById(int patientid) 
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
            GetPatientsInfo(out patientList);//获取所有患者信息
            GetPatientByIdInfo(patientid, out patientList);//获取某个患者信息
            patient =patientList.FirstOrDefault();
            GetMedicalRecordByPatientId(patientid, out medicalRecord);//获取某个患者病历记录

              // 创建一个包含患者姓名和医疗记录的视图模型
              var viewModel = new Tuple<List<MedicalRecord>, Patient>(medicalRecord, patient);
            return View(viewModel);
           
        }
        
        public virtual void GetMedicalRecordByPatientId(int patientId, out List<MedicalRecord> medicalRecordList)
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
        }//某种患者病历记录
        //问诊、录入病历CreateMedicalRecord
        public IActionResult CreateMedicalRecord()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateMedicalRecord(int appointmentId,MedicalRecord medicalRecord)
        {
            return View();
        }

        //开检查单
        public IActionResult CreateCheckOrder()
        {
            //// 准备检查项目下拉列表（显示 Name + Description，值使用 CheckItemID）
            //var checkItems = _context.CheckItems
            //    .Where(ci => ci.IsActive)
            //    .Select(ci => new
            //    {
            //        CheckItemID = ci.CheckItemID,
            //        DisplayText = ci.Name + (string.IsNullOrEmpty(ci.Description) ? "" : " - " + ci.Description)
            //    })
            //    .ToList();

            //ViewBag.CheckItemID = new SelectList(checkItems, "CheckItemID", "DisplayText");

            //// 预约下拉列表保持不变（可根据需要调整）
            //ViewBag.AppointmentID = new SelectList(_context.Appointments, "AppointmentID", "AppointmentID");
            // 准备检查项目下拉列表（显示 Name + Description，值使用 CheckItemID）
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckOrder checkOrder)
        {
            //if (ModelState.IsValid)
            //{
            //    checkOrder.CreatedAt = DateTime.Now;
            //    checkOrder.Status = "已开单";
            //    _context.Add(checkOrder);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}

            //// 如果验证失败，需要重新填充下拉列表（同样使用组合文本）
            //var checkItems = _context.CheckItems
            //    .Where(ci => ci.IsActive)
            //    .Select(ci => new
            //    {
            //        CheckItemID = ci.CheckItemID,
            //        DisplayText = ci.Name + (string.IsNullOrEmpty(ci.Description) ? "" : " - " + ci.Description)
            //    })
            //    .ToList();
            //ViewBag.CheckItemID = new SelectList(checkItems, "CheckItemID", "DisplayText", checkOrder.CheckItemID);
            //ViewBag.AppointmentID = new SelectList(_context.Appointments, "AppointmentID", "AppointmentID", checkOrder.AppointmentID);
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

        // GET: CheckOrders/Index
        public IActionResult CheckOrdersIndex()
        {
            // 模拟检查单数据（基于您提供的示例）
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

            // 构建 AppointmentID 到患者姓名的字典（假设 AppointmentID == PatientID）
            var patientDict = patients.ToDictionary(p => p.PatientID, p => p.Name);
            // 检查项目数据（复用之前的 _checkItems）
            // 为了在视图中方便获取项目名称，可以将检查项目列表存入 ViewBag 或使用 Join
            ViewBag.CheckItems = _checkItems.ToDictionary(ci => ci.CheckItemID, ci => ci.Name);

            return View(checkOrders);
        }
        #endregion
    }
}
