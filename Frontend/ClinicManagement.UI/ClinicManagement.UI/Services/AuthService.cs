using ClinicManagement.UI.DTOs;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClinicManagement.UI.Services
{
    public class AuthService : BaseApiService
    {
        public AuthService() : base()
        {
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest { Email = email, MatKhau = password };

                // Gọi API
                var response = await PostAsync<LoginRequest, LoginResponse>("auth/login", request);

                if (response != null)
                {
                    // Cập nhật AppState (Singleton)
                    // Lưu ý: Đảm bảo các property trong AppState đã có OnPropertyChanged()
                    AppState.Instance.AuthToken = response.Token;
                    AppState.Instance.CurrentUserName = response.HoTen;
                    AppState.Instance.CurrentUserRole = response.VaiTro;
                    AppState.Instance.CurrentUserRoleCode = response.VaiTroCode;

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Ghi log để bạn biết chính xác tại sao login thất bại
                // Mở cửa sổ Output trong VS để xem log này khi debug
                Debug.WriteLine($"[AuthService] Lỗi đăng nhập: {ex.Message}");

                // Có thể ném lại lỗi hoặc thông báo lên UI qua một Message Service nếu cần
            }
            return false;
        }

        public void Logout()
        {
            // Reset thông tin khi đăng xuất
            AppState.Instance.AuthToken = null;
            AppState.Instance.CurrentUserName = string.Empty;
            AppState.Instance.CurrentUserRole = string.Empty;
        }
    }
}