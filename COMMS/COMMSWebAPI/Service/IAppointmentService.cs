using COMMSWebAPI.Models;
using COMMSWebAPI.Models.Response;

namespace COMMSWebAPI.Service
{
    public interface IAppointmentService
    {
        Task<AppointmentResponse> GetAppointmentsAsync(int? deptId, int? doctorId);//加载预约挂号首页信息
        Task<object> GetAppointmentsAsyncObject(int? deptId, int? doctorId);//加载预约挂号首页信息
        Task<string> CreateAppointmentAsync(int patientId, int scheduleId);//创建预约挂号

        Task<object> GetAppointmentHistory(int? patientID);
        Task<string> CreatePayment(Payments paymentRequst);
        Task<object> GetAppointmentQueue(int? doctorId, DateTime? date);
        Task<object> GetSchedule(int? doctorId, DateTime? date);
        Task<string> UpdatescheduleAsync(int scheduleId, int? doctorId, int? maxAppointments);
        Task<string> UpdateAppointmentAsync(int appointmentId, int? scheduleId, string status);
    }
}
