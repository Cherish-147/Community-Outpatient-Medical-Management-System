using COMMS.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace COMMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly String _connectionString;
        private readonly DBHelper db = new DBHelper();
        //private static readonly ILog log =LogManager.GetLogger(typeof(HomeController));
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult Index()
        {
            TestLog();
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


        public void TestLog()
        {
            string msg = string.Empty;
            msg = "index";
            log.Info(msg);
            log.Debug("哈哈哈");
            log.Error("哈哈哈");
            log.InfoFormat("哈哈哈{0}", msg);
        }
        public ActionResult TestConnectionDB()
        {
            var dbHelper = new DBHelper();

            bool result = dbHelper.TestConnection();
            // 将结果传递给视图
            ViewBag.ConnectionResult = result ? "数据库连接成功" : "数据库连接失败";
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginRequest request)
        {
            string message = string.Empty;
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                message = "用户名或者密码不能为空!";
                ViewBag.ConnectionResult = message;
                log.Error(message);
                return View();
            }
            try
            {
                // 对输入的密码进行哈希加密
                string hashedInputPassword = HashPassword(request.Password);
                // 修改SQL查询，只查询用户名，然后比较哈希值
                string sql = "SELECT Password, UserID FROM Users WHERE UserName = @UserName";
                SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@UserName",request.UserName)
                };
                var result = db.ExecuteQuery(sql, parameters);
                if (result.Rows.Count > 0)
                {
                    string storedHashedPassword = result.Rows[0]["Password"].ToString();
                    // 比较哈希值
                    if (storedHashedPassword == hashedInputPassword)
                    {
                        int userID = Convert.ToInt32(result.Rows[0]["UserID"]);
                        message = "登录成功!";
                        log.Info($"用户 {request.UserName} 登录成功，用户ID: {userID}");
                        Session["UserID"] = userID;
                        Session["UserName"] = request.UserName;
                    }
                    else
                    {
                        message = "密码错误!";
                        log.Warn($"用户 {request.UserName} 登录失败：密码错误");
                    }
                }
                else
                {
                    message = "用户名不存在!";
                    log.Warn($"登录失败：用户名 {request.UserName} 不存在");
                }
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                ViewBag.ConnectionResult = message;
            }
            ViewBag.ConnectionResult = message;
            return View();
        }
        //哈希加密
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));// 使用十六进制格式，每个字节2个字符
                }
                return sb.ToString();
            };
        }

        [HttpGet]
        public ActionResult UserRegister()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserRegister(RegisterRequest registerRequest)
        {
            string message=string.Empty;
            try
            {
                string hashedPassword = HashPassword(registerRequest.Password);
                string userid =string.Empty;
                string querySql = "select count(*) +1 From Users";
                var queryResult = db.ExecuteQuery(querySql);
                userid = queryResult.Rows[0][0].ToString();

                if (string.IsNullOrEmpty(userid))
                {
                    return Content(string.Empty);
                }
                string insertSql = "insert into Users(UserName,Password,Role,Email,PhoneNumber,Gender)" +
                    "values (@UserName,@Password,@Role,@Email,@PhoneNumber,@Gender);" +
                    "select SCOPE_IDENTITY()";

                SqlParameter[] insertParameters =
                {
                    new SqlParameter("@UserName",registerRequest.UserName),
                    new SqlParameter("@Password",hashedPassword),
                    new SqlParameter("@Role",registerRequest.Role),
                    new SqlParameter("@Email",registerRequest.Email),
                    new SqlParameter("@PhoneNumber",registerRequest.PhoneNumber),
                    new SqlParameter("@Gender",registerRequest.Gender),
                };
                var insertResult = db.ExecuteQuery(insertSql,insertParameters);
                if (insertResult.Rows.Count>0)
                {
                    int newUserID = Convert.ToInt32(insertResult.Rows[0][0]);
                    message = $"你的用户ID为:{newUserID},注册成功!\n请保住你的用户ID!";
                    log.Info($":{message}");

                }
            }
            catch (Exception ex)
            {
                message = ex.ToString();
               
            }
            ViewBag.ContentResult = message;
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            log.Info("用户退出登录");
            return RedirectToAction("Index");
        }
    }
}