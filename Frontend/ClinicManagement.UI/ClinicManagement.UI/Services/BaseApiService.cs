using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClinicManagement.UI.Services
{
    public class BaseApiService
    {
        // Tên biến HttpClient trong dự án của bạn là Client
        protected static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://clinic-management-api-w2up.onrender.com/api/"),
            Timeout = TimeSpan.FromSeconds(30)
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
                var response = await Client.PostAsJsonAsync(endpoint, data);

                // Nếu Server xử lý thành công (Mã 200)
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                // 🌟 CHÌA KHÓA PHÁ ÁN: Nếu dính lỗi 400, 403, 500... bốc chuỗi giải thích từ Server ra log
                var errorContent = await response.Content.ReadAsStringAsync();
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
                var response = await Client.PutAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] PUT Lỗi Server: {errorContent}");
                return default;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BaseApiService] Lỗi kết nối PUT: {ex.Message}");
                return default;
            }
        }
    }
}