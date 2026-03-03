using Microsoft.AspNetCore.Mvc;

namespace COMMSMVC.Controllers
{
    public class PatientController : Controller
    {
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
    }
}
