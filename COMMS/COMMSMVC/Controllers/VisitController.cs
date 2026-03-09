using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace COMMSMVC.Controllers
{
    public class VisitController : Controller
    {
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

        //查看患者挂号
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
            return View(patients);
           
        }
        //查看某个患者所有信息
        public IActionResult GetPatientById(int patientid)
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
            return View(patient);
        }
        //查看某个患者病历记录
        public IActionResult GetPatientMedicalRecordById(int patientid) 
        {
            
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
    } };
            var patient = patients.FirstOrDefault();
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
            // 创建一个包含患者姓名和医疗记录的视图模型
            var viewModel = new Tuple<List<MedicalRecord>, Patient>(medicalRecord, patient);
            return View(viewModel);
           
        }
       
        #endregion
    }
}
