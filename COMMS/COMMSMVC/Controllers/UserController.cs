using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;


namespace COMMSMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly string baseUrl = "https://localhost:7190/api";
        private HttpClient httpclient = new();
    
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";

        public async Task<IActionResult> Index()//狭取所有用户信息
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")) ||
                    (HttpContext.Session.GetString("Role") != "Admin" && HttpContext.Session.GetString("Role") != "Staff"))
            {
                return RedirectToAction("Login", "Home");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                //token 为空
                return RedirectToAction("Login", "Home");
            }
            DataTable dataUsers = new DataTable();
            dataUsers.Columns.Add("UserId", typeof(int));
            dataUsers.Columns.Add("UserName", typeof(string));
            dataUsers.Columns.Add("Password", typeof(string));
            dataUsers.Columns.Add("Role", typeof(string));
            dataUsers.Columns.Add("Email", typeof(string));
            dataUsers.Columns.Add("PhoneNumber", typeof(string));
            dataUsers.Columns.Add("Gender", typeof(string));
            dataUsers.Columns.Add("IsActive", typeof(bool));

            try
            {
                var token = HttpContext.Session.GetString("JWTToken");
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var usersRes = await client.GetAsync(baseUrl + "/UserManage/GetUsersrsList");
                var userResBody = await usersRes.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<Users>>(userResBody);

                foreach (var user in users)
                {
                    dataUsers.Rows.Add(
                        user.UserId,
                        user.UserName,
                        user.Password,
                        user.Role,
                        user.Email,
                        user.PhoneNumber,
                        user.Gender,
                        user.IsActive);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "加载用户列表失败：" + ex.Message;
            }
            return View(dataUsers);
        }


        public async Task<IActionResult> Detai1s(int? id)
        {
            if (id == null)
            {
                return BadRequest("ID cannot be null");
            }
            var token = HttpContext.Session.GetString("JWTToken");
            using (httpclient)
            {
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var usersRes = await httpclient.GetAsync(baseUrl + " /UserManage/GetUserById / " + id);
                var userResBody = await usersRes.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<Users>(userResBody);
                if (user == null)
                    return NotFound("ID Not Find");
                return View(user);
            }


            //if(id == null) {return new HttpStatusCodeResult(HttpStatusCode.BadRequest);}
            //Users users = db.Temp1113_Users.Find(id);
            //if(users = null)
            //return HttpNotFound();
            //return View(users);
        }
        public ActionResult Create()
        {
            AddDropdownList();
            return View();
        }

        private void AddDropdownList(string? selectedRole = null)
        {
            var roles = new List<SelectListItem> {
            new SelectListItem { Text="Admin",Value = "Admin" },
            new SelectListItem { Text="User",Value = "User" },
            new SelectListItem { Text="Manager",Value = "Manager" }};
            ViewBag.RoleList = new SelectList(roles, "Value", "Text", selectedRole);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Users users)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (ModelState.IsValid)
            {
                using (httpclient)
                {
                    httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var json = System.Text.Json.JsonSerializer.Serialize(users);
                    var content = new StringContent(json, Encoding.UTF8, "text/json");
                    var response = await httpclient.PostAsync(baseUrl + "/UserManage/CreateUser", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            AddDropdownList();
            return View(users);
        }


        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            //AddDropdownList();
            // 构造下拉选项
            ViewBag.RoleList = new SelectList(new List<string> { "Admin", "Doctor", "Patient" });
            if (id == null)
            {
                return BadRequest("ID cannot be nu11");
            }
            var token = HttpContext.Session.GetString("JWTToken");
            using (httpclient)
            {
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var usersRes = await httpclient.GetAsync(baseUrl + "/UserManage/GetUserData/" + id);

                var userResBody = await usersRes.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(userResBody);
                if (users == null)
                {
                    return NotFound("ID Not Find");
                }
                var user = users.First();
                return View(user);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserModel model)
        {
            try
            {
                // 基础验证
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.PhoneNumber) || string.IsNullOrEmpty(model.Role))
                {
                    ViewBag.Error = "用户姓名、角色、联系电话为必填项！";
                    return View(model);
                }
                var newPassword = string.Empty;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    newPassword = HashPassword(model.Password);
                }
                using var client = new HttpClient();
                var token = HttpContext.Session.GetString("JwtToken");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var requestData = new
                {
                    UserName = model.UserName,
                    Gender = string.IsNullOrEmpty(model.Gender) ? DBNull.Value : (object)model.Gender,
                    UserID = model.UserId,
                    Password=newPassword,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Role = string.IsNullOrEmpty(model.Role) ? DBNull.Value : (object)model.Role,
                    IsActive = model.IsActive ? true : false
                };
                // 序列化为 JSON
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //发送 POST 请求
                var response = await client.PutAsync(baseUrl + $"/UserManage/UpdateUser/{model.UserId}", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if (bool.TryParse(responseBody, out bool success) && success)
                    {
                        TempData["Success"] = "用户信息更新成功！";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Error = responseBody;
                    }
                }
                else
                {
                    ViewBag.Error = response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "新增用户出错：" + ex.Message;
            }
            return View(model);
        }

        /// <summary>
        /// 删除
        /// </summary>
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
                    // 执行删除
                    string deleteSql = "DELETE FROM dbo.Users WHERE UserId = @UserId";
                    SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
                    deleteCmd.Parameters.AddWithValue("@UserId", id);

                    int rows = deleteCmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "用户信息删除成功！";
                    }
                    else
                    {
                        TempData["Error"] = "删除用户信息失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "删除用户信息出错：" + ex.Message;
            }

            return RedirectToAction("Index");
        }


        public IActionResult CreateUser()
        {
            return View();
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateUser(Users model)
        {
            try
            {
                // 基础验证
                if (string.IsNullOrEmpty(model.Password)  || string.IsNullOrEmpty(model.UserName)||string.IsNullOrEmpty(model.PhoneNumber))
                {
                    ViewBag.Error = "用户姓名、密码和联系电话为必填项！";
                    return View(model);
                }
                if(!model.Password.Equals(model.TwoPassword))
                {
                    ViewBag.Error = "两次输入的密码不一致！";
                    return View(model);
                }
                string Password = string.Empty;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    Password = HashPassword(model.Password);
                }
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"
                        INSERT INTO dbo.Users (UserName, Password, PhoneNumber, Gender, IsActive,Role)
                        VALUES (@UserName, @Password, @PhoneNumber, @Gender, @IsActive,@Role)";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserName", model.UserName);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(model.Gender) ? DBNull.Value : (object)model.Gender);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? true : false); // 默认启用
                    cmd.Parameters.AddWithValue("@Role", string.IsNullOrEmpty(model.Role) ? DBNull.Value : (object)model.Role);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        TempData["Success"] = "用户信息新增成功！";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Error = "新增用户失败，请重试！";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "新增用户出错：" + ex.Message;
            }

            return View(model);
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
            }
            ;
        }
    }
    public class UserModel
    {
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
    }
    }
