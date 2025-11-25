using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using COMMS.Models;

namespace COMMS.Controllers
{
    public class UsersController : Controller
    {
        private MyDbContext db = new MyDbContext();

        // GET: Users
        public async Task<ActionResult> Index()
        {
            return View(await db.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = await db.Users.FindAsync(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性。有关
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "UserId,UserName,Password,Role,Email,PhoneNumber,Gender")] Users users)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(users);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = await db.Users.FindAsync(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: Users/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性。有关
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserId,UserName,Password,Role,Email,PhoneNumber,Gender")] Users users)
        {
            if (ModelState.IsValid)
            {
                db.Entry(users).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = await db.Users.FindAsync(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Users users = await db.Users.FindAsync(id);
            db.Users.Remove(users);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult AppointmentIndex(int? deptid, int? doctorId)
        {
            ViewBag.Departments = db.Departments.ToList();
            ViewBag.Doctors = deptid.HasValue ? db.Doctors.Where(d => d.DeptID == deptid.Value).ToList() : new System.Collections.Generic.List<Doctor>();
            ViewBag.Schedule = doctorId.HasValue ? db.Schedules.Where(s => s.DoctorID == doctorId.Value).ToList() : new System.Collections.Generic.List<Schedule>();
            return View();

        }

        [HttpPost]
        public ActionResult MakeAppointment(int patientId,int scheduleId)
        {
            using (db)
            {
                var appointment = new Appointment
                {
                    PatientID = patientId,
                    ScheduleID = scheduleId,
                    Status = "已预约",
                    CreatedAt = DateTime.Now,
                };
                db.Appointments.Add(appointment);
                db.SaveChanges();

                return RedirectToAction("Confirm",new {id=appointment.AppointmentID});
            }
        }

        public ActionResult Confirm(int id)
        {
            using (db) { 
            var appointment = db.Appointments.Find(id);
                return View(appointment);

            }    
        }
    }
}
