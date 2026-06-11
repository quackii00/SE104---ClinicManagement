using System;

namespace ClinicManagement.UI.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Hàm giúp lấy đúng mật khẩu dù Frontend gửi field nào
        public string GetMatKhau() => !string.IsNullOrEmpty(MatKhau) ? MatKhau : (Password ?? string.Empty);
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string VaiTroCode { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
    }

    public class MeResponse
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string VaiTroCode { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
    }
}