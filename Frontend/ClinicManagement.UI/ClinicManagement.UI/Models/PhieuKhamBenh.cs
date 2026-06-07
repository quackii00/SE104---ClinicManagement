using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.Models
{
    public class Thuoc
    {
        public string MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal DonGia { get; set; }
        public string MaDonVi { get; set; }
    }

    public class CachDung
    {
        public string MaCachDung { get; set; }
        public string MoTaCachDung { get; set; }
    }

    public class ChiTietToaThuoc
    {
        public Thuoc Thuoc { get; set; }
        public CachDung CachDung { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class PhieuKhamBenh
    {
        public string MaPhieuKham { get; set; }
        public DateTime NgayKham { get; set; }
        public string TrieuChung { get; set; }
        public string MaLoaiBenh { get; set; }
        public string TenLoaiBenh { get; set; }

        public BenhNhan BenhNhanKham { get; set; }
        public List<ChiTietToaThuoc> ChiTietToaThuoc { get; set; } = new List<ChiTietToaThuoc>();

        public bool ThemChiTietToaThuoc(Thuoc thuoc, int soLuong, CachDung cachDung)
        {
            if (soLuong <= 0) return false;

            var chiTiet = new ChiTietToaThuoc
            {
                Thuoc = thuoc,
                SoLuong = soLuong,
                CachDung = cachDung,
                DonGia = thuoc.DonGia
            };

            ChiTietToaThuoc.Add(chiTiet);
            return true;
        }
    }
}