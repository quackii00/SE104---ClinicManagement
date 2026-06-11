using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            // Đường dẫn gốc của API Backend.
            // Cổng HTTPS local của Backend (xem Properties/launchSettings.json) là 7089.
            // Khi deploy hãy đổi sang URL công khai (Render) của nhóm.
            BaseAddress = new Uri("https://localhost:7089/api/"),
            Timeout = TimeSpan.FromSeconds(30) // Quá 30 giây không phản hồi thì ngắt kết nối
        };

        /// <summary>
        /// Đính kèm JWT (nếu đã đăng nhập) vào header Authorization trước mỗi request.
        /// Các endpoint quy định/báo cáo phía Backend đều yêu cầu [Authorize] nên bước này là bắt buộc.
        /// </summary>
        private static void ApplyAuthHeader()
        {
            var token = AppState.Instance.AuthToken;
            Client.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Hàm dùng chung để gửi yêu cầu lấy dữ liệu từ Server (GET)
        /// </summary>
        protected async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                ApplyAuthHeader();
                HttpResponseMessage response = await Client.GetAsync(endpoint);

                // Nếu Server trả về mã 200-299 thành công, tự động đọc và ép kiểu JSON sang DTO
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                throw new HttpRequestException(await BuildErrorAsync(response));
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
                ApplyAuthHeader();
                HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                throw new HttpRequestException(await BuildErrorAsync(response));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService POST Error]: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Hàm dùng chung để gửi dữ liệu CẬP NHẬT lên Server (PUT).
        /// Dùng cho các thao tác sửa đổi bản ghi đã tồn tại (vd: cập nhật quy định/tham số).
        /// </summary>
        protected async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                ApplyAuthHeader();
                HttpResponseMessage response = await Client.PutAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                throw new HttpRequestException(await BuildErrorAsync(response));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService PUT Error]: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gom mã lỗi + nội dung body (thường là {"message": "..."} do Backend trả về)
        /// thành một thông điệp dễ đọc để ViewModel hiển thị cho người dùng.
        /// </summary>
        private static async Task<string> BuildErrorAsync(HttpResponseMessage response)
        {
            string body = string.Empty;
            try { body = await response.Content.ReadAsStringAsync(); } catch { /* bỏ qua */ }

            return string.IsNullOrWhiteSpace(body)
                ? $"Lỗi hệ thống từ Server: {(int)response.StatusCode} {response.StatusCode}"
                : $"Lỗi từ Server ({(int)response.StatusCode}): {body}";
        }
    }
}
