namespace COMMSMVC.Properties.Configurations
{
    public static class AppConfig
    {
        private static readonly IConfiguration _configuration;

        static AppConfig()
        {
            // 构建配置
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径为当前目录
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 读取 appsettings.json
                .Build();
        }

        // 直接提供静态属性读取配置值
        public static string BaseUrl => _configuration["ApiSettings:BaseUrl"];
    }
}
