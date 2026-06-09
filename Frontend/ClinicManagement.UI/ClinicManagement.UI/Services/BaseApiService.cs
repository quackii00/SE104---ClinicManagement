using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Lớp dịch vụ nền tảng (Base) cấu hình HttpClient dùng chung.
    /// Tất cả các Service chức năng khác sẽ kế thừa từ đây để gửi/nhận dữ liệu với Backend.
    /// </summary>
    public class BaseApiService
    {
        // Khởi tạo một HttpClient độc nhất (static) để dùng xuyên suốt chu kỳ chạy app
        protected static readonly HttpClient Client = new HttpClient
        {
            // Đường dẫn gốc của API Backend. Huyền nhớ đổi lại đúng cổng (Port) của Backend nhóm mình nha!
            BaseAddress = new Uri("https://localhost:7000/api/"),
            Timeout = TimeSpan.FromSeconds(30) // Quá 30 giây không phản hồi thì ngắt kết nối
        };

        /// <summary>
        /// Hàm dùng chung để gửi yêu cầu lấy dữ liệu từ Server (GET)
        /// </summary>
        protected async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync(endpoint);

                // Nếu Server trả về mã 200-299 thành công, tự động đọc và ép kiểu JSON sang DTO
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                throw new HttpRequestException($"Lỗi hệ thống từ Server: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService GET Error]: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Hàm dùng chung để gửi dữ liệu lên Server xử lý (POST)
        /// </summary>
        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                throw new HttpRequestException($"Lỗi hệ thống từ Server: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService POST Error]: {ex.Message}");
                throw;
            }
        }
    }
}