using System;

namespace ClinicManagement.UI.DTOs
{
    /// <summary>
    /// Gói đăng nhập gửi LÊN Backend (POST api/auth/login).
    /// Backend chấp nhận field "Password" (alias của MatKhau) nên Frontend gửi Email + Password.
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Phản hồi đăng nhập từ Backend. Tương ứng 1-1 với LoginResponse bên Server.
    /// Token chính là JWT sẽ được lưu vào AppState để các request sau gửi kèm.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;

        /// <summary>Mã code vai trò: Admin / BacSi / TiepTan / KeToan.</summary>
        public string VaiTroCode { get; set; } = string.Empty;

        /// <summary>Tên hiển thị vai trò tiếng Việt (vd: "Bác sĩ").</summary>
        public string VaiTro { get; set; } = string.Empty;
    }
}
