using System;

namespace ClinicManagement.UI.Models
{
    public class HoaDonThanhToan
    {
        public string MaHoaDon { get; set; }
        public decimal TienKham { get; set; }
        public decimal TienThuoc { get; set; }
        public decimal TongTien { get; set; }
        public bool TrangThaiThanhToan { get; set; }

        public PhieuKhamBenh PhieuKhamBenh { get; set; }

        public void TinhTienKham(decimal tienKhamDinhMuc)
        {
            TienKham = tienKhamDinhMuc;
        }

        public void TinhTienThuoc()
        {
            if (PhieuKhamBenh == null || PhieuKhamBenh.ChiTietToaThuoc == null)
            {
                TienThuoc = 0;
                return;
            }

            decimal tongTienThuoc = 0;
            foreach (var chiTiet in PhieuKhamBenh.ChiTietToaThuoc)
            {
                if (chiTiet.Thuoc != null)
                {
                    tongTienThuoc += chiTiet.SoLuong * chiTiet.DonGia;
                }
            }
            TienThuoc = tongTienThuoc;
        }

        public decimal TinhTongTien()
        {
            TongTien = TienKham + TienThuoc;
            return TongTien;
        }

        public bool XuatHoaDon()
        {
            if (PhieuKhamBenh == null) return false;
            TrangThaiThanhToan = true;
            return true;
        }
    }
}