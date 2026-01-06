using COMMSWebAPI.Models;
using COMMSWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMMSWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppointmentController(MyDbContext _db, IHomeService _iHomeService, IAuthService _iAuthService
        , IAppointmentService _iAppointmentService) : ControllerBase
    {
        private MyDbContext db = _db;
        public IHomeService iHomeService = _iHomeService;
        public IAuthService iAuthService = _iAuthService;
        public IAppointmentService iAppointmentService = _iAppointmentService;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAppointments(int? deptId, int? doctorId)
        {
            var result = await iAppointmentService.GetAppointmentsAsync(deptId, doctorId);
            return Ok(result);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAppointmentsObject(int? deptId, int? doctorId)
        {
            var result = await iAppointmentService.GetAppointmentsAsyncObject(deptId, doctorId);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAppointmentIndex0(int? deptId, int? doctorId)
        {
            var departments = await db.Departments.ToListAsync();

            var doctors = deptId.HasValue ? await db.Doctors.Where(d => d.DeptID == deptId.Value).ToListAsync()
                : new List<Doctor>();

            var schedules = doctorId.HasValue
            ? await db.Schedules.Where(s => s.DoctorID == doctorId.Value).ToListAsync()
            : [];

            var result = new
            {
                Departments = departments,
                Doctors = doctors,
                Schedules = schedules
            };


            return Ok(result);//json
        }

        [HttpGet]
        [AllowAnonymous]
        
        public async Task<IActionResult> GetAppointmentHistory(int? patientID)
        {

            var result = await iAppointmentService.GetAppointmentHistory(patientID);
            return Ok(result);
        }
    }
}
