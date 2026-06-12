using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    public class BaseApiService
    {
        /// <summary>
        /// Thông điệp lỗi gần nhất do Server trả về (lấy từ body { "message": ... } khi POST/PUT thất bại).
        /// ViewModel đọc giá trị này để hiện đúng lý do cho người dùng thay vì im lặng.
        /// </summary>
        public string? LastErrorMessage { get; private set; }

        // Tên biến HttpClient trong dự án của bạn là Client.
        // URL gốc KHÔNG hard-code: đọc từ appsettings.json qua AppConfig (xem Services/AppConfig.cs).
        protected static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl),
            // Render gói free "ngủ" khi không dùng -> request đầu tiên có thể chờ vài chục giây để server thức dậy.
            Timeout = TimeSpan.FromSeconds(60)
        };

        private void PrepareAuthHeader()
        {
            var token = AppState.Instance.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        protected async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                PrepareAuthHeader();
                var response = await Client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                return default;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Lỗi GET: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 🌟 ĐÃ SỬA: Bọc try-catch an toàn cho POST để hứng lỗi 400 Bad Request và in chi tiết lỗi
        /// </summary>
        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                PrepareAuthHeader();
                LastErrorMessage = null;
                var response = await Client.PostAsJsonAsync(endpoint, data);

                // Nếu Server xử lý thành công (Mã 200)
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                // 🌟 CHÌA KHÓA PHÁ ÁN: Nếu dính lỗi 400, 403, 500... bốc chuỗi giải thích từ Server ra log
                var errorContent = await response.Content.ReadAsStringAsync();
                LastErrorMessage = ExtractMessage(errorContent);
                System.Diagnostics.Debug.WriteLine("======================= [SERVER BAD REQUEST LOG] =======================");
                System.Diagnostics.Debug.WriteLine($"[API Error tại Endpoint {endpoint}]: Mã lỗi {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[Chi tiết lỗi từ Backend]: {errorContent}");
                System.Diagnostics.Debug.WriteLine("========================================================================");

                return default; // Trả về null để ViewModel biết đường xử lý chặn giao diện, không làm sập app
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Lỗi kết nối POST bất ngờ: {ex.Message}");
                return default;
            }
        }

        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest requestData)
        {
            try
            {
                PrepareAuthHeader();
                LastErrorMessage = null;
                var response = await Client.PutAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                LastErrorMessage = ExtractMessage(errorContent);
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] PUT Lỗi Server: {errorContent}");
                return default;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Lỗi kết nối PUT: {ex.Message}");
                return default;
            }
        }
        protected async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                PrepareAuthHeader();
                var response = await Client.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] DELETE Lỗi Server tại {endpoint}: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Lỗi kết nối DELETE: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa và trả về (thành công?, thông điệp). Khi bị backend chặn (vd: đang được dùng trong phiếu khám)
        /// thì lấy đúng lý do từ body { "message": ... } để ViewModel hiện cho người dùng, thay vì im lặng.
        /// </summary>
        protected async Task<(bool Success, string Message)> DeleteWithMessageAsync(string endpoint, string defaultSuccessMessage)
        {
            try
            {
                PrepareAuthHeader(); // 🌟 FIX: trước đây các lệnh xóa gọi thẳng Client.DeleteAsync nên thiếu token
                var response = await Client.DeleteAsync(endpoint);
                var body = await response.Content.ReadAsStringAsync();
                var serverMsg = ExtractMessage(body);

                if (response.IsSuccessStatusCode)
                    return (true, string.IsNullOrWhiteSpace(serverMsg) ? defaultSuccessMessage : serverMsg!);

                System.Diagnostics.Debug.WriteLine($"[BaseApiService] DELETE bị chặn tại {endpoint}: {body}");
                return (false, string.IsNullOrWhiteSpace(serverMsg) ? "Thao tác xóa thất bại." : serverMsg!);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Lỗi kết nối DELETE: {ex.Message}");
                return (false, $"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }

        /// <summary>Bóc trường "message" từ body JSON do Server trả về (an toàn với body rỗng / không phải JSON).</summary>
        private static string? ExtractMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                var parsed = JsonSerializer.Deserialize<MessageResponse>(body,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return string.IsNullOrWhiteSpace(parsed?.Message) ? null : parsed!.Message;
            }
            catch
            {
                return null;
            }
        }
    }
}