using COMMSMVC.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OfficeOpenXml;// EPPlus
using PuppeteerSharp;
using System.Data.SqlClient;

namespace COMMSMVC.Controllers
{
    public class PaymentController : Controller
    {
        private readonly string _connectionString = "Server=.;Database=Community-Outpatient-Medical-Management-System;Integrated Security=true;Encrypt=False;";
        private readonly string baseUrl ="";
        public PaymentController(IOptions<ApiConfig> apiConfig, IConfiguration configuration)
        {
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
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
               await conn.OpenAsync();
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
        public async Task<IActionResult> Create(int? appointmentID)
        {
            var appointments = await GetAppointmentsForPaymentSelectAsync();
            var viewModel = new CreatePaymentViewModel
            {
                Payment = new Payment(),
                AppointmentList = new SelectList(appointments, "Value", "Text")
            };
            // 如果传入了 appointmentID，则预选该预约
            if (appointmentID.HasValue)
            {
                viewModel.Payment.AppointmentID = appointmentID.Value;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePaymentViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    // 验证失败，重新加载下拉列表
            //    var appointments = await GetAppointmentsForPaymentSelectAsync();
            //    model.AppointmentList = new SelectList(appointments, "Value", "Text");
            //    return View(model);
            //}

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> Delete(int id)
        {
            bool success = await DeleteAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "支付记录删除成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "删除失败，请稍后重试。";
            }
            return RedirectToAction(nameof(Index));

        }
        public virtual async Task<bool> DeleteAsync(int paymentID)
        {
            string sql = "DELETE FROM [Payments] WHERE PaymentID = @PaymentID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", paymentID);
                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // 至少删除一行才算成功
                }
            }
            catch (Exception ex)
            {
                // 记录日志（建议使用 ILogger）
                // _logger.LogError(ex, "删除支付记录失败，PaymentID: {PaymentID}", paymentID);
                return false;
            }
        }

        #region 打印
        /// <summary>
        /// 导出支付详情到 Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int id)
        {

            // 获取支付详情数据
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();

            // 创建 Excel 文件
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("支付详情");
                // 设置列宽
                worksheet.Cells[1, 1].Value = "支付ID";
                worksheet.Cells[1, 2].Value = "预约ID";
                worksheet.Cells[1, 3].Value = "患者姓名";
                worksheet.Cells[1, 4].Value = "金额";
                worksheet.Cells[1, 5].Value = "支付方式";
                worksheet.Cells[1, 6].Value = "状态";
                worksheet.Cells[1, 7].Value = "支付时间";

                worksheet.Cells[2, 1].Value = payment.PaymentID;
                worksheet.Cells[2, 2].Value = payment.AppointmentID;
                worksheet.Cells[2, 3].Value = payment.PatientName;
                worksheet.Cells[2, 4].Value = payment.Amount;
                worksheet.Cells[2, 5].Value = payment.Method;
                worksheet.Cells[2, 6].Value = payment.Status;
                worksheet.Cells[2, 7].Value = payment.PaidAt.ToString("yyyy-MM-dd HH:mm");

                // 自动调整列宽
                worksheet.Cells[1, 1, 2, 7].AutoFitColumns();

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"支付详情_{id}.xlsx");
            }
        }

        /// <summary>
        /// 导出支付详情到 PDF
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();

            using (var stream = new MemoryStream())
            {
                // 创建 PDF 文档
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                // 添加标题
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("支付详情", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);
                doc.Add(new Paragraph(" ")); // 空行

                // 创建表格（7列）
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;

                // 添加行
                AddPdfRow(table, "支付ID", payment.PaymentID.ToString());
                AddPdfRow(table, "预约ID", payment.AppointmentID.ToString());
                AddPdfRow(table, "患者姓名", payment.PatientName);
                AddPdfRow(table, "金额", payment.Amount.ToString("C"));
                AddPdfRow(table, "支付方式", payment.Method ?? "-");
                AddPdfRow(table, "状态", payment.Status ?? "-");
                AddPdfRow(table, "支付时间", payment.PaidAt.ToString("yyyy-MM-dd HH:mm"));

                doc.Add(table);
                doc.Close();

                return File(stream.ToArray(), "application/pdf", $"支付详情_{id}.pdf");
            }
        }

        private void AddPdfRow(PdfPTable table, string label, string value)
        {
            var labelCell = new PdfPCell(new Phrase(label, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)))
            {
                BackgroundColor = new BaseColor(240, 240, 240),
                Padding = 8
            };
            var valueCell = new PdfPCell(new Phrase(value, FontFactory.GetFont(FontFactory.HELVETICA, 12)))
            {
                Padding = 8
            };
            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }

        /// <summary>
        /// 导出支付详情为图片（使用 PuppeteerSharp 将当前详情页转换为图片）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToImage(int id)
        {
            // 1. 获取支付详情数据（用于构建完整页面）
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();

            // 2. 构建要渲染的 HTML（可重用 Detail 视图的部分，但这里简单构建）
            string htmlContent = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>支付详情</title>
            <style>
                body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; }}
                .container {{ max-width: 800px; margin: auto; border: 1px solid #ddd; border-radius: 8px; padding: 20px; }}
                h2 {{ text-align: center; color: #0d6efd; }}
                table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                td {{ padding: 8px; border-bottom: 1px solid #dee2e6; }}
                .label {{ font-weight: bold; background-color: #f8f9fa; width: 30%; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>支付详情</h2>
                <table>
                    <tr><td class='label'>支付ID</td><td>{payment.PaymentID}</td></tr>
                    <tr><td class='label'>预约ID</td><td>{payment.AppointmentID}</td></tr>
                    <tr><td class='label'>患者姓名</td><td>{payment.PatientName}</td></tr>
                    <tr><td class='label'>金额</td><td>{payment.Amount:C}</td></tr>
                    <tr><td class='label'>支付方式</td><td>{payment.Method ?? "-"}</td></tr>
                    <tr><td class='label'>状态</td><td>{payment.Status ?? "-"}</td></tr>
                    <tr><td class='label'>支付时间</td><td>{payment.PaidAt:yyyy-MM-dd HH:mm}</td></tr>
                </table>
            </div>
        </body>
        </html>";

            // 3. 使用 PuppeteerSharp 渲染图片
            await new BrowserFetcher().DownloadAsync(); // 首次运行会下载 Chromium（约100MB），建议提前下载或部署时包含
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(htmlContent);
                var screenshot = await page.ScreenshotDataAsync(); // 默认 PNG 格式
                return File(screenshot, "image/png", $"支付详情_{id}.png");
            }
        }
        // GET: 显示支付确认页面
        [HttpGet]
        public async Task<IActionResult> ConfirmPay(int id)
        {
            // 根据 PaymentID 查询支付记录，验证状态是否为“待支付”
            // 此处仅为示例，实际应调用服务层方法
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null || payment.Status != "待支付")
            {
                TempData["ErrorMessage"] = "该支付记录不存在或已支付！";
                //return RedirectToAction("MyPayments");
            }
            return View(payment);
            //// 模拟支付成功，更新状态为“已支付”
            //bool success = await UpdatePaymentStatusAsync(id, "已支付");
            //if (success)
            //{
            //    TempData["SuccessMessage"] = "支付成功！";
            //}
            //else
            //{
            //    TempData["ErrorMessage"] = "支付失败，请稍后重试。";
            //}
            //return RedirectToAction(nameof(PatientController.MyPayments), "Patient");
        }

        // POST: 执行支付
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPay(int paymentID,string paymentMethod)
        {
            var payment = await GetPaymentByIdAsync(paymentID);
            if (payment == null || payment.Status != "待支付")
            {
                TempData["ErrorMessage"] = "支付记录无效或已支付。";
                return RedirectToAction("MyPayments", "Patient");
            
        }
            string payStatus = "已支付";
            // 更新支付状态、支付方式、支付时间
            bool success = await UpdatePaymentStatusAsync(paymentID, payStatus, paymentMethod);
            if (success)
            {
                TempData["SuccessMessage"] = "支付成功！";
                return RedirectToAction( "MyPayments","Patient");
            }
            else
            {
                TempData["ErrorMessage"] = "支付失败，请稍后重试。";
                return RedirectToAction("ConfirmPay", new { paymentID });
            }
        }
        // 辅助方法：更新支付状态（需实现）
        private async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status,string payMethod)
        {
            string sql = "UPDATE Payments SET [Status] = @Status,Method=@Method WHERE PaymentID = @PaymentID";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Method", payMethod);
                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                await conn.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
        #endregion
}

