using System;

namespace ClinicManagement.UI.DTOs
{
    // ==========================================
    // 1. CÁC GÓI DỮ LIỆU NHẬN VỀ TỪ API (GET)
    // ==========================================

    /// <summary>
    /// DTO hiển thị danh mục Loại bệnh (LB1..LB5)
    /// </summary>
    public class LoaiBenhDto
    {
        public int Id { get; set; }
        public string MaLoaiBenh { get; set; } = string.Empty;
        public string TenLoaiBenh { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO hiển thị danh mục Đơn vị tính (Viên, Chai, Vỉ...)
    /// </summary>
    public class DonViDto
    {
        public int Id { get; set; }
        public string MaDonVi { get; set; } = string.Empty;
        public string TenDonVi { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO hiển thị danh mục Cách dùng thuốc (CD01, CD02...)
    /// </summary>
    public class CachDungDto
    {
        public int Id { get; set; }
        public string MaCachDung { get; set; } = string.Empty;
        public string MoTaCachDung { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO hiển thị danh mục Thuốc đầy đủ thông tin kèm đơn giá
    /// </summary>
    public class ThuocDto
    {
        public int Id { get; set; }
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public string MaDonVi { get; set; } = string.Empty;
        public string TenDonVi { get; set; } = string.Empty;
    }

    // ==========================================
    // 2. CÁC GÓI YÊU CẦU GỬI LÊN API (POST/PUT)
    // ==========================================

    /// <summary>
    /// YC6 – Yêu cầu thêm/sửa danh mục Loại Bệnh
    /// </summary>
    public class UpsertLoaiBenhRequest
    {
        public string TenLoaiBenh { get; set; } = string.Empty;
        public string? MaLoaiBenh { get; set; }
    }

    /// <summary>
    /// YC6 – Yêu cầu thêm/sửa danh mục Thuốc
    /// </summary>
    public class UpsertThuocRequest
    {
        public string TenThuoc { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public string MaDonVi { get; set; } = string.Empty;
        public string? MaThuoc { get; set; }
    }

    /// <summary>
    /// YC6 – Yêu cầu thêm/sửa danh mục Đơn Vị Tính
    /// </summary>
    public class UpsertDonViRequest
    {
        public string TenDonVi { get; set; } = string.Empty;
        public string? MaDonVi { get; set; }
    }

    /// <summary>
    /// YC6 – Yêu cầu thêm/sửa danh mục Cách Dùng
    /// </summary>
    public class UpsertCachDungRequest
    {
        public string MoTaCachDung { get; set; } = string.Empty;
        public string? MaCachDung { get; set; }
    }
}