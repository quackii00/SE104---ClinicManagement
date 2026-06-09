using System;

namespace ClinicManagement.UI.DTOs
{
    public class ChiTietKhamDto
    {
        // Nhận trực tiếp số thứ tự khám do Server Backend tính toán và cấp phát
        public int STT { get; set; }

        // Nhận trạng thái do Server quản lý (Ví dụ: "Chờ khám", "Đã khám", "Đang khám")
        public string TrangThai { get; set; }

        public string MaBenhNhan { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public int NamSinh { get; set; }
        public string DiaChi { get; set; }
    }
}