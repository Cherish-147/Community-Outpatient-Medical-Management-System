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
        CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0),
        Remark = "常规检查"
    },
    new AppointmentViewModel
    {
        AppointmentID = 2,
        PatientID = 2,
        PatientName = "赵六",
        ScheduleID = 2,
        Status = "已到诊",
        CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0),
        Remark = "术后复查"
    },
    new AppointmentViewModel
    {
        AppointmentID = 3,
        PatientID = 3,
        PatientName = "钱七",
        ScheduleID = 3,
        Status = "已看诊",
        CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0),
        Remark = "感冒发烧"
    },
    new AppointmentViewModel
    {
        AppointmentID = 4,
        PatientID = 4,
        PatientName = "孙八",
        ScheduleID = 4,
        Status = "已完成",
        CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0),
        Remark = "妇科检查"
    },
    new AppointmentViewModel
    {
        AppointmentID = 5,
        PatientID = 5,
        PatientName = "吴十",
        ScheduleID = 5,
        Status = "取消",
        CreatedAt = new DateTime(2024, 1, 9, 0, 0, 0),
        Remark = "患者临时有事"
    },
    new AppointmentViewModel
    {
        AppointmentID = 6,
        PatientID = 6,
        PatientName = "郑十一",
        ScheduleID = 6,
        Status = "已预约",
        CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0),
        Remark = "耳鼻喉检查"
    },
    new AppointmentViewModel
    {
        AppointmentID = 7,
        PatientID = 7,
        PatientName = "王十二",
        ScheduleID = 7,
        Status = "已到诊",
        CreatedAt = new DateTime(2024, 1, 11, 0, 0, 0),
        Remark = "牙齿疼痛"
    },
    new AppointmentViewModel
    {
        AppointmentID = 8,
        PatientID = 1,
        PatientName = "王五",
        ScheduleID = 11,
        Status = "已取消",
        CreatedAt = new DateTime(2026, 3, 4, 1, 27, 45, 347),
        Remark = "发骚"
    },
    new AppointmentViewModel
    {
        AppointmentID = 9,
        PatientID = 13,
        PatientName = "2",
        ScheduleID = 13,
        Status = "已预约",
        CreatedAt = new DateTime(2026, 3, 8, 11, 37, 24, 197),
        Remark = "1"
    },
    new AppointmentViewModel
    {
        AppointmentID = 10,
        PatientID = 13,
        PatientName = "2",
        ScheduleID = 14,
        Status = "已取消",
        CreatedAt = new DateTime(2026, 3, 8, 11, 41, 44, 880),
        Remark = "扭到脚了"
    }
};
            #endregion
            return View(appointments);
        }
        #endregion
    }
}
