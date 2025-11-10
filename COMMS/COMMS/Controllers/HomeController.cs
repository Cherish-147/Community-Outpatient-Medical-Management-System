using COMMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace COMMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly String _connectionString;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        


        public ActionResult TestConnectionDB()
        {
            var dbHelper = new DBHelper();
           
            bool  result = dbHelper.TestConnection();
            // 将结果传递给视图
            ViewBag.ConnectionResult = result ? "数据库连接成功" : "数据库连接失败";
            return View();
        }
        public ActionResult Login()
        {
            //var dbHelper = new DBHelper();
            //bool  result = dbHelper.TestConnection();
            //// 将结果传递给视图
            //ViewBag.ConnectionResult = result ? "数据库连接成功" : "数据库连接失败";
            return View();
        }
    }
}