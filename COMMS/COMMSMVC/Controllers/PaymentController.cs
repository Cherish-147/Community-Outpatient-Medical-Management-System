using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class PaymentController : Controller
    {
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";

        //支付控制器
        public async Task<IActionResult> Index()//控制器
        {
           var paymentsIndex =await GetPaymentsList();
            return View(paymentsIndex);
        }

        public virtual async Task<List<PaymentsIndex>> GetPaymentsList()
        {
            var paymentIndexList = new List<PaymentsIndex>();
            string sql = @"
        SELECT pay.PaymentID, pay.AppointmentID,
               p.PatientID, p.Name AS PatientName,
               pay.Amount, pay.Method, pay.Status, pay.PaidAt
        FROM Payments pay
        INNER JOIN Appointments a ON pay.AppointmentID = a.AppointmentID
        INNER JOIN Patients p ON a.PatientID = p.PatientID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.OpenAsync();
                using (SqlDataReader reader =await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        paymentIndexList.Add(new PaymentsIndex
                        {
                            PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            Method = reader.IsDBNull(reader.GetOrdinal("Method")) ? null : reader.GetString(reader.GetOrdinal("Method")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                            PaidAt = reader.GetDateTime(reader.GetOrdinal("PaidAt"))
                        });
                    }
                }
            }
            return paymentIndexList;
        }
    }
}
