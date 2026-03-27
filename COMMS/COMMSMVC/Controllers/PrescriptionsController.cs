using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class PrescriptionsController : Controller
    {
        #region 开处方控制器
        private readonly ILogger<PrescriptionsController> _logger;
        //private string baseUrl = "https://localhost:7190/api";
        private string baseUrl;
        //private string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        private string _connectionString;
        public PrescriptionsController(ILogger<PrescriptionsController> logger, IOptions<ApiConfig> apiConfig, IConfiguration configuration)
        {
            _logger = logger;
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
 #endregion
        [HttpGet]
        public async Task<IActionResult> Index()//控制器，处方列表首页
        {
            var prescriptionsList =await  GetPrescriptionsIndexInfoAsync();
            return View(prescriptionsList);
        }

        public virtual async Task<List<PrescriptionsIndexModel>> GetPrescriptionsIndexInfoAsync()//方法，获取处方列表信息
        {
            var prescriptionsList = new List<PrescriptionsIndexModel>();
            string sql = @"
                            select pr.PrescriptionID,pr.AppointmentID,pr.CreatedAt
                            ,prd.DetailID
                            ,prd.MedicationID
                            ,prd.DoseValue
                            ,prd.DoseUnit
                            ,prd.Quantity
                            ,prd.Frequency
                            ,prd.Duration
                            ,p.Name as patientName
                            ,m.Name as medicationName
                            from Prescriptions pr
                            inner join PrescriptionDetails prd on pr.PrescriptionID =prd.PrescriptionID
                            inner join Appointments a on  pr.AppointmentID =a.AppointmentID
                            inner join Patients p on a.PatientID =p.PatientID
                            inner join Medications m on prd.MedicationID=m.MedicationID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var model = new PrescriptionsIndexModel
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                                MedicationID = reader.GetInt32(reader.GetOrdinal("MedicationID")),
                                DoseValue = reader.GetDecimal(reader.GetOrdinal("DoseValue")),
                                DoseUnit = reader.IsDBNull(reader.GetOrdinal("DoseUnit")) ? null : reader.GetString(reader.GetOrdinal("DoseUnit")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Frequency = reader.IsDBNull(reader.GetOrdinal("Frequency")) ? null : reader.GetString(reader.GetOrdinal("Frequency")),
                                Duration = (int)(reader.IsDBNull(reader.GetOrdinal("Duration")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Duration"))),
                                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                MedicationName = reader.GetString(reader.GetOrdinal("MedicationName"))
                            };
                            prescriptionsList.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志（建议使用 ILogger）
                Console.WriteLine($"获取处方列表失败：{ex.Message}");
                // 根据需要可以重新抛出或返回空列表
                return null;
            }
            return prescriptionsList;
        }


    }
}
