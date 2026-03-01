using Microsoft.AspNetCore.Mvc;

namespace COMMSMVC.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult PatientIndex()
        {
            //// 获取当前登录患者ID
            //var patientId = Context.Session.GetString("UserID");
            //// 查询患者基本信息
            //var patientInfo = _patientService.GetPatientInfo(patientId);
            //// 查询最近就诊记录
            //var recentRecords = _recordService.GetRecentRecords(patientId, 3);
            //// 查询常用医生
            //var commonDoctors = _doctorService.GetCommonDoctors(patientId);
            //// 查询就医提醒
            //var reminders = _reminderService.GetPatientReminders(patientId);

            //ViewData["PatientInfo"] = patientInfo;
            //ViewData["RecentRecords"] = recentRecords;
            //ViewData["CommonDoctors"] = commonDoctors;
            //ViewData["Reminders"] = reminders;

            return View();
        }
    }
}
