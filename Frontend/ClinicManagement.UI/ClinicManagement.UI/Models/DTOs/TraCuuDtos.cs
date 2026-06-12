using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.DTOs
{
    /// <summary>
    /// Kết quả trả về khi tìm kiếm nâng cao ở màn hình Tra Cứu
    /// </summary>
    public class TraCuuBenhNhanResultDto
    {
        public int STT { get; set; }
        public string MaBenhNhan { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = string.Empty;
        public int NamSinh { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime NgayKham { get; set; }
        public string? TenLoaiBenh { get; set; }
        public string? TrieuChung { get; set; }
    }

    /// <summary>Thông tin hồ sơ bệnh nhân trả về khi tra theo SĐT (tự điền form Tiếp nhận).</summary>
    public class BenhNhanInfoDto
    {
        public string MaBenhNhan { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = string.Empty;
        public int NamSinh { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
    }

    /// <summary>
    /// Chi tiết lịch sử một ca khám trong quá khứ của bệnh nhân
    /// </summary>
    public class LichSuKhamDto
    {
        public string MaPhieuKham { get; set; } = string.Empty;
        public DateTime NgayKham { get; set; }
        public string? TrieuChung { get; set; }
        public string? TenLoaiBenh { get; set; }
        public string BactorName { get; set; } = string.Empty; // Tên bác sĩ lập phiếu
        public List<ChiTietToaThuocDto> ToaThuoc { get; set; } = new List<ChiTietToaThuocDto>();
    }
}