using COMMSWebAPI.Models;
using COMMSWebAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMMSWebAPI.Service
{
    public class AppointmentService(MyDbContext _db) : IAppointmentService
    {
        private readonly MyDbContext db = _db;

        public async Task<object> GetAppointmentsAsyncObject(int? deptId, int? doctorId)
        {
            var departments = await db.Departments.ToListAsync();
            var doctors = deptId.HasValue ? await db.Doctors.Where(d => d.DeptID == deptId.Value).ToListAsync() : new List<Doctor>();
            var schedules = doctorId.HasValue ? await db.Schedules.Where(s => s.DoctorID == doctorId.Value).ToListAsync() : [];
            return new
            {
                Departments = departments,
                Doctors = doctors,
                Schedules = schedules
            };
        }

        public async Task<AppointmentResponse> GetAppointmentsAsync(int? deptId, int? doctorId)
        {
            var departments = await db.Departments.ToListAsync();
            var doctors = deptId.HasValue ? await db.Doctors.Where(d => d.DeptID == deptId.Value).ToListAsync() : new List<Doctor>();
            var schedules = doctorId.HasValue ? await db.Schedules.Where(s => s.DoctorID == doctorId.Value).ToListAsync() : [];
            return new AppointmentResponse
            {
                Departments = departments,
                Doctors = doctors,
                Schedules = schedules
            };


        }
        [HttpPost]
        public async Task<string> CreateAppointmentAsync(int patientId, int scheduleId)
        {
            try
            {
                using (db)
                {
                    // 检查是否己存在预约
                    var exists = await db.Appointments
                        .AnyAsync(a => a.PatientID == patientId && a.ScheduleID == scheduleId);
                    if (exists)
                    {
                        return "该患者已预约该时间段，请勿重复预约!";
                    }
                    var appointment = new Appointment
                    {
                        PatientID = patientId,
                        ScheduleID = scheduleId,
                        Status = "已预约",
                        CreatedAt = DateTime.Now
                    };
                    db.Appointments.Add(appointment); await db.SaveChangesAsync(); return "Create Appointment Successfully";
                }
            }
            catch (Exception ex)
            {
                return "Create Appointment Error:\n" + ex.ToString();
            }
        }

        public async Task<object> GetAppointmentHistory(int? patientID)//all appointment History By UserId or all
        {
            var appointHistory = await (from u in db.Users
                                        join ap in db.Appointments
                                        on u.UserID equals ap.PatientID
                                        where !patientID.HasValue || ap.PatientID == patientID
                                        orderby ap.CreatedAt descending
                                        select ap).ToListAsync();
            return appointHistory;
            //select * from[Users] u inner join Appointments ap on u.UserID=ap.PatientID 
            //    where PatientID = 2
        }

        public async Task<string> CreatePayment(Payments paymentRequst)
        {
            string msg;
            var appointment = await db.Appointments.FindAsync(paymentRequst.AppointmentID);
            if (appointment == null) return ("Appointment not found.");
            var pament = new Payments
            {
                PaymentID = 0,
                AppointmentID = paymentRequst.AppointmentID,
                Amount = paymentRequst.Amount,
                Method = paymentRequst.Method,
                Status = "already Pay",
                PaidAt = DateTime.Now
            };

            db.Payments.Add(pament);
            var affectedRows = await db.SaveChangesAsync();
            int newPaymentId = pament.PaymentID;
            if (affectedRows > 0)
            {
                msg = $"affectedRows:{affectedRows} Faild Create ID: {newPaymentId} Payment";
            }
            msg = $"PaymentID :{newPaymentId} Create Successfully";
            return msg;
        }

        public async Task<object> GetAppointmentQueue(int? doctorId, DateTime? date)
        {
            var q = from ap in db.Appointments
                    join s in db.Schedules on ap.ScheduleID equals s.ScheduleID
                    where (!doctorId.HasValue || s.DoctorID == doctorId.Value)
                    && (!date.HasValue || ap.CreatedAt == date.Value.Date)
                    orderby ap.CreatedAt descending
                    select ap;
            //declare @DoctorId int = NULL
            //declare @Date datetime = NULL
            //SELECT* FROM Appointments ap
            //inner join Schedules s on s.ScheduleID = ap.ScheduleID
            //WHERE(@DoctorId IS NULL OR DoctorId = @DoctorId)
            //AND(@Date IS NULL OR CAST(CreatedAt AS DATE)= @Date)
            //ORDER BY CreatedAt DEsC;
            //0R 的作用:只要其中一个条件成立，整条 WHERE 条件就成立。
            //当 @DoctorId IS NULL →整个表达式为 TRUE →不加医生过滤。
            //当 @DoctorId 有值→s.DoctorId = @DoctorId 必须成立 》按医生筛选。            var result = await q.ToListAsync(); return result;
            var result = await q.ToListAsync();
            return result;
        }

        public async Task<object> GetSchedule(int? doctorId, DateTime? date)
        {
            //DECLARE @DoctorId INT = 3;
            //DECLARE @Date DATE = '2025-12-01';
            //SELECT ScheduleID, s.Date, s.TimeSlot, s.MaxAppointments, d.Name, d.Title
            //FROM Schedules s
            //INNER JOIN Doctors d ON s.DoctorID = d.DoctorID
            //WHERE(@DoctorId IS NULL OR s.DoctorID = @DoctorId)AND(@Date IS NULL OR s.Date = @Date)
            //ORDER BY s.Date DESC;
            var q = from s in db.Schedules
                    join d in db.Doctors on s.DoctorID equals d.DoctorID
                    where (!doctorId.HasValue || s.DoctorID == doctorId.Value)
                    && (!date.HasValue || s.Date == date.Value.Date)
                    orderby s.Date descending
                    select new
                    {
                        s.ScheduleID,
                        s.Date,
                        s.TimeSlot,
                        s.MaxAppointments,
                        doctorName = d.DoctorName,
                        d.Title
                    };
            var result = await q.ToListAsync();
            return result;
        }

        public async Task<string> UpdatescheduleAsync(int scheduleId, int? doctorId, int? maxAppointments)
        {

            //declare @scheduleID int =10;
            //declare @DoctorID    int = 3:
            //declare @MaxAppointments int =10;
            //UPDATE Schedules
            //SET DOCtOrID = CASE WHEN @DOCtOrID IS NOT NULL THEN @DOCtOrID ELSE DOCtorID END.
            //MaxAppointments = CASE WHEN @MaxAppointments IS NOT NULL THEN @MaxAppointments ELSE MaxAppointments END
            //WHERE ScheduleID = @scheduleID;
            var schedule = await db.Schedules.FindAsync(scheduleId);
            if (schedule == null) { return $"Not Find scheduleId:{scheduleId}"; }
            if (doctorId.HasValue) schedule.DoctorID = doctorId.Value;
            if (maxAppointments.HasValue) schedule.MaxAppointments = maxAppointments.Value;

            var affectedRow = await db.SaveChangesAsync();
            if (affectedRow < 1) return "update Schedule Failed!s";

            return "update Schedule Successfully!";
        }
        public async Task<string> UpdateAppointmentAsync(int appointmentId, int? scheduleId, string status)
        {

            //UPDATE Appointments//同时更新状态和排班，更新挂号关联的排班(比如换医生
            //SET Status='已取消',ScheduleID =13
            //WHERE AppointmentID =1001 :
            var appointment = await db.Appointments.FindAsync(appointmentId) ?? throw new KeyNotFoundException($"appointmentId:{appointmentId} Not Find");
            if (!string.IsNullOrEmpty(status)) appointment.Status = status;
            if (scheduleId.HasValue)
                appointment.ScheduleID = scheduleId.Value;
            var affectedRow = await db.SaveChangesAsync();
            if (affectedRow < 1)
                return "update Appointment Failed!";
            return "update Appointment Successfully!";
        }

    }
}
