using COMMSMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace COMMSMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly string baseUrl = "https://localhost:7117/api";
        private HttpClient httpclient = new();
        //private tokens = HttpContext.Session.Getstring("JWTToken");//不能全局

        public async Task<IActionResult> Index()//狭取所有用户信息
        {
            var token = HttpContext.Session.GetString("JWTToken");
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",token);
            var usersRes = await client.GetAsync(baseUrl + "/Users/GetAllUser");
            var userResBody = await usersRes.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<Users>>(userResBody);
            return View(users);//return View(db.Temp1113 Users.ToList());
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
                var usersRes = await httpclient.GetAsync(baseUrl + " /Users /GetUserById / " + id);
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
                    httpclient.DefaultRequestHeaders.Authorization=  new AuthenticationHeaderValue("Bearer", token);
                    var json = System.Text.Json.JsonSerializer.Serialize(users);
                    var content = new StringContent(json, Encoding.UTF8, "text/json");
                    var response = await httpclient.PostAsync(baseUrl + "/Users/CreateUser", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            AddDropdownList();
            return View(users);
        }

        //[HttpPost]

        //public async Task<ActionResult> Create([Bind(Include = "UserId,UserName,Password,Role,Email,PhoneNumber,Gender")] Users users)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Users.Add(users);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //AddDropdownList();//法-:翻这个留视图@Htm1.DropDownlistFor(model=>model.Role),@*法一  new List<SelectListItem> 
        //    return View(users);
        //}

        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            AddDropdownList();
            if (id == null)
            {
                return BadRequest("ID cannot be nu11");
            }
            var token =HttpContext.Session.GetString("JWTToken");
            using (httpclient)
            {
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var usersRes = await httpclient.GetAsync(baseUrl + "/Users/GetUserById/" + id);

                var userResBody = await usersRes.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<Users>(userResBody);
                if (user == null)
                {
                    return NotFound("ID Not Find");
                }
                return View(user);
            } 
        }
        }

    }
