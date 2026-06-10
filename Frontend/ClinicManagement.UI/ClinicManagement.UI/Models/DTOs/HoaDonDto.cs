using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.DTOs
{
    public class HoaDonDto
    {
        public string MaHoaDon { get; set; }
        public string MaPhieuKham { get; set; }
        public string MaBenhNhan { get; set; }
        public string HoTen { get; set; }
        public DateTime NgayKham { get; set; }
        public decimal TienKham { get; set; }
        public decimal TienThuoc { get; set; }
        public decimal TongTien { get; set; }
        public bool DaThanhToan { get; set; }
        public List<ChiTietToaThuocDto> ChiTietThuoc { get; set; } = new List<ChiTietToaThuocDto>();

        // 🌟 Tạo thuộc tính này để ánh xạ chuẩn với Binding {Binding TongTotal} trong InvoiceViewModel
        public decimal TongTotal => TongTien;
    }

    
}