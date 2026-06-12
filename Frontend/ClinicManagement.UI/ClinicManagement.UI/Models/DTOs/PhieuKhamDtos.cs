using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.DTOs
{
    /// <summary>
    /// Gói dữ liệu gửi lên API khi Bác sĩ nhấn Hoàn tất khám
    /// </summary>
    public class CreatePhieuKhamRequest
    {
        public string MaBenhNhan { get; set; } = string.Empty;
        public DateTime NgayKham { get; set; } = DateTime.Today;
        public string? TrieuChung { get; set; }
        public string? MaLoaiBenh { get; set; }
        public List<ChiTietToaThuocRequest> ToaThuoc { get; set; } = new List<ChiTietToaThuocRequest>();
    }

    /// <summary>
    /// Chi tiết từng dòng thuốc trong đơn gửi lên API
    /// </summary>
    public class ChiTietToaThuocRequest
    {
        public string MaThuoc { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public string MaCachDung { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gói dữ liệu nhận về từ API sau khi lập phiếu thành công
    /// </summary>
    public class PhieuKhamDto
    {
        public string MaPhieuKham { get; set; } = string.Empty;
        public string MaBenhNhan { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgayKham { get; set; }
        public string? TrieuChung { get; set; }
        public string? MaLoaiBenh { get; set; }
        public string? TenLoaiBenh { get; set; }
        public bool DaLapHoaDon { get; set; }
        public List<ChiTietToaThuocDto> ToaThuoc { get; set; } = new List<ChiTietToaThuocDto>();
    }

    /// <summary>
    /// Chi tiết thông tin thuốc hiển thị (Dùng chung cho cả xem lại đơn thuốc và hiển thị hóa đơn)
    /// </summary>
    public class ChiTietToaThuocDto
    {
        public string MaThuoc { get; set; } = string.Empty;
        public string TenThuoc { get; set; } = string.Empty;
        public string TenDonVi { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public string MaCachDung { get; set; } = string.Empty;
        public string MoTaCachDung { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }


    }
}