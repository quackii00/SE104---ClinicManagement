using System;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Đọc cấu hình ứng dụng từ appsettings.json (+ biến môi trường).
    /// Mục đích: KHÔNG hard-code URL/secret trong mã nguồn.
    /// File appsettings.json bị .gitignore (mỗi máy/môi trường tự cấu hình theo appsettings.example.json),
    /// nên URL Production không bị đẩy lên GitHub.
    /// </summary>
    public static class AppConfig
    {
        private static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)                       // thư mục chứa file .exe khi chạy
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()                                   // cho phép override bằng biến môi trường (Api__BaseUrl)
            .Build();

        /// <summary>
        /// URL gốc của API Backend (kết thúc bằng "/api/").
        /// Thứ tự ưu tiên: appsettings.json (Api:BaseUrl) → biến môi trường → mặc định chạy local.
        /// Lưu ý: giá trị mặc định chỉ là localhost (không phải secret) để app vẫn chạy được khi thiếu file cấu hình.
        /// </summary>
        public static string ApiBaseUrl =>
            Configuration["Api:BaseUrl"] is { Length: > 0 } url
                ? url
                : "https://localhost:7089/api/";
    }
}
