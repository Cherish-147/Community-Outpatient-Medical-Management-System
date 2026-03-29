using COMMSMVC.Models;
using COMMSMVC.Models.Statistics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace COMMSMVC.Controllers
{
    public class StatisticsController : Controller
    {
        #region 数据统计控制器

        #endregion
        private  string _connectionString;
        private  string baseUrl = "";
        public StatisticsController(IOptions<ApiConfig> apiConfig,IConfiguration configuration)
        {
            baseUrl = apiConfig.Value.BaseUrl;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            //return RedirectToAction(nameof(MedicationStatistics));
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> MedicationStatistics()
        {
            var viewModel = new MedicationChartViewModel
            {
                Names = new List<string>(),
                Stocks = new List<int>(),
                Prices = new List<decimal>()
            };

            string sql = @"
            SELECT Name, Stock, Price 
            FROM Medications 
            WHERE IsActive = 1 
            ORDER BY Name";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        viewModel.Names.Add(reader.GetString(reader.GetOrdinal("Name")));
                        viewModel.Stocks.Add(reader.GetInt32(reader.GetOrdinal("Stock")));
                        viewModel.Prices.Add(reader.GetDecimal(reader.GetOrdinal("Price")));
                    }
                }
            }

            return View(viewModel);
        }
    }
}
