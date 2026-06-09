using System;

namespace ClinicManagement.UI.DTOs
{
    /// <summary>
    /// Gói dữ liệu (Request) gom thông tin từ Form Tiếp Nhận trên giao diện 
    /// để chuẩn bị đóng gói gửi lên Backend Controller xử lý.
    /// </summary>
    public class DangKyKhamRequest
    {
        // Các thông tin thô Tiếp tân nhập từ bàn phím theo đúng YC1
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public int NamSinh { get; set; }
        public string DiaChi { get; set; }

        // Ngày đăng ký khám (Mặc định là ngày hôm nay)
        public DateTime NgayKham { get; set; }
    }
}