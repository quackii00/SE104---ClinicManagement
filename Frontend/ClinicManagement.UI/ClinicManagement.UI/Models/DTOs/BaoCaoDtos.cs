using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.DTOs
{
    /// <summary>BM5.1 – Một dòng báo cáo doanh thu theo ngày (khớp DoanhThuItemDto Backend).</summary>
    public class DoanhThuItemDto
    {
        public int STT { get; set; }
        public DateTime Ngay { get; set; }
        public int SoBenhNhan { get; set; }
        public decimal DoanhThu { get; set; }

        /// <summary>Tỷ lệ doanh thu của ngày trên tổng doanh thu tháng (%).</summary>
        public double TyLe { get; set; }
    }

    /// <summary>BM5.1 – Báo cáo doanh thu tháng (GET api/baocao/doanhthu).</summary>
    public class BaoCaoDoanhThuDto
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongSoBenhNhan { get; set; }
        public List<DoanhThuItemDto> ChiTiet { get; set; } = new();
    }

    /// <summary>BM5.2 – Một dòng báo cáo sử dụng thuốc (khớp SuDungThuocItemDto Backend).</summary>
    public class SuDungThuocItemDto
    {
        public int STT { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public int SoLuong { get; set; }

        /// <summary>Số lần thuốc được kê (số phiếu khám có dùng thuốc này).</summary>
        public int SoLanDung { get; set; }
    }

    /// <summary>BM5.2 – Báo cáo sử dụng thuốc tháng (GET api/baocao/sudungthuoc).</summary>
    public class BaoCaoSuDungThuocDto
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public List<SuDungThuocItemDto> ChiTiet { get; set; } = new();
    }
}
