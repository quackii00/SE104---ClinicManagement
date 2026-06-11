using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Service xác thực – gọi POST api/auth/login để lấy JWT.
    /// Kế thừa BaseApiService để dùng chung HttpClient (endpoint login là [AllowAnonymous]).
    /// </summary>
    public class AuthService : BaseApiService
    {
        /// <summary>
        /// Đăng nhập:
        /// - Trả về LoginResponse (kèm Token) nếu thành công.
        /// - Trả về null nếu sai email/mật khẩu (Server trả 401).
        /// - Ném exception nếu lỗi mạng / không kết nối được Server (để ViewModel báo riêng).
        /// </summary>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Dùng Client trực tiếp để phân biệt rõ "sai mật khẩu" (401) với "mất kết nối".
            using HttpResponseMessage response = await Client.PostAsJsonAsync("auth/login", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return null; // Sai email hoặc mật khẩu

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
    }
}
