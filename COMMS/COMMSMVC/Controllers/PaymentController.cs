using COMMSMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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


        // GET: /Payments/Detail/5
        public async Task<IActionResult> Detail(int id)//控制器
        {
            // 调用数据访问方法获取单个支付记录
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(); // 未找到则返回 404
            }
            return View(payment);
        }

        public virtual async Task<PaymentsIndex> GetPaymentByIdAsync(int paymentId)//方法，查看某个付款记录
        {
            string sql = @"
        SELECT pay.PaymentID, pay.AppointmentID,
               p.PatientID, p.Name AS PatientName,
               pay.Amount, pay.Method, pay.Status, pay.PaidAt
        FROM Payments pay
        INNER JOIN Appointments a ON pay.AppointmentID = a.AppointmentID
        INNER JOIN Patients p ON a.PatientID = p.PatientID
        WHERE pay.PaymentID = @PaymentID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new PaymentsIndex
                        {
                            PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            Method = reader.IsDBNull(reader.GetOrdinal("Method")) ? null : reader.GetString(reader.GetOrdinal("Method")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                            PaidAt = reader.GetDateTime(reader.GetOrdinal("PaidAt"))
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }


        // GET: /Payments/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await GetPaymentByIdAsyncEdit(id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // POST: /Payments/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentsIndex model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool success = await UpdatePaymentAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "支付记录更新成功！";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "更新失败，请稍后重试。";
                return View(model);
            }
        }


        public virtual async Task<PaymentsIndex> GetPaymentByIdAsyncEdit(int paymentId)
        {
            string sql = @"
        SELECT pay.PaymentID, pay.AppointmentID,
               p.PatientID, p.Name AS PatientName,
               pay.Amount, pay.Method, pay.Status, pay.PaidAt
        FROM Payments pay
        INNER JOIN Appointments a ON pay.AppointmentID = a.AppointmentID
        INNER JOIN Patients p ON a.PatientID = p.PatientID
        WHERE pay.PaymentID = @PaymentID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new PaymentsIndex
                        {
                            PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                            AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            Method = reader.IsDBNull(reader.GetOrdinal("Method")) ? null : reader.GetString(reader.GetOrdinal("Method")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                            PaidAt = reader.GetDateTime(reader.GetOrdinal("PaidAt"))
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public virtual async Task<bool> UpdatePaymentAsync(PaymentsIndex payment)
        {
            string updateSql = @"
        UPDATE Payments
        SET Method = @Method,
            Status = @Status,
            Amount = @Amount
        WHERE PaymentID = @PaymentID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@Method", payment.Method ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", payment.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                cmd.Parameters.AddWithValue("@PaymentID", payment.PaymentID);

                await conn.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var appointments = await GetAppointmentsForPaymentSelectAsync();
            var viewModel = new CreatePaymentViewModel
            {
                Payment = new Payment(),
                AppointmentList = new SelectList(appointments, "Value", "Text")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 验证失败，重新加载下拉列表
                var appointments = await GetAppointmentsForPaymentSelectAsync();
                model.AppointmentList = new SelectList(appointments, "Value", "Text");
                return View(model);
            }

            // 设置支付时间为当前时间
            model.Payment.PaidAt = DateTime.Now;

            bool success = await InsertPaymentAsync(model.Payment);
            if (success)
            {
                TempData["SuccessMessage"] = "支付记录创建成功！";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "创建失败，请稍后重试。";
                // 重新加载下拉列表并返回视图
                var appointments = await GetAppointmentsForPaymentSelectAsync();
                model.AppointmentList = new SelectList(appointments, "Value", "Text");
                return View(model);
            }
        }
        public virtual async Task<bool> InsertPaymentAsync(Payment payment)
        {
            string insertSql = @"
        INSERT INTO Payments (AppointmentID, Amount, Method, Status, PaidAt)
        VALUES (@AppointmentID, @Amount, @Method, @Status, @PaidAt)";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", payment.AppointmentID);
                cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                cmd.Parameters.AddWithValue("@Method", payment.Method ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", payment.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PaidAt", payment.PaidAt);

                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
        public virtual async Task<List<SelectListItem>> GetAppointmentsForPaymentSelectAsync()
        {
            string sql = @"
        SELECT a.AppointmentID, p.Name AS PatientName
        FROM Appointments a
        INNER JOIN Patients p ON a.PatientID = p.PatientID
        ORDER BY a.AppointmentID";

            var items = new List<SelectListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("AppointmentID"));
                        string name = reader.GetString(reader.GetOrdinal("PatientName"));
                        items.Add(new SelectListItem
                        {
                            Value = id.ToString(),
                            Text = $"预约ID:{id} - {name}"
                        });
                    }
                }
            }
            return items;
        }
    }
}
