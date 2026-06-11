using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.DTOs
{
    public class DoanhThuItemDto
    {
        public int STT { get; set; }
        public DateTime Ngay { get; set; }
        public int SoBenhNhan { get; set; }
        public decimal DoanhThu { get; set; }
        public double TyLe { get; set; }
    }

    public class BaoCaoDoanhThuDto
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongSoBenhNhan { get; set; }
        public List<DoanhThuItemDto> ChiTiet { get; set; } = new List<DoanhThuItemDto>();
    }

    public class SuDungThuocItemDto
    {
        public int STT { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public int SoLanDung { get; set; }
    }

    public class BaoCaoSuDungThuocDto
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public List<SuDungThuocItemDto> ChiTiet { get; set; } = new List<SuDungThuocItemDto>();
    }
}