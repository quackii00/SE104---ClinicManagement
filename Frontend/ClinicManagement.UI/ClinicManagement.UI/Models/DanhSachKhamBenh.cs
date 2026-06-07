using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.UI.Models
{
    public class ChiTietDanhSachKham
    {
        public int STT { get; set; }
        public string TrangThai { get; set; }
        public BenhNhan BenhNhan { get; set; }
    }

    public class DanhSachKhamBenh
    {
        public int Id { get; set; }
        public DateTime NgayKham { get; set; }
        public int SoLuongHienTai => ChiTietDanhSach?.Count ?? 0;
        public List<ChiTietDanhSachKham> ChiTietDanhSach { get; set; }
        public bool KiemTraGioiHan(int soLuongToiDa)
        {
            return SoLuongHienTai < soLuongToiDa;
        }

        public ChiTietDanhSachKham ThemBenhNhan(BenhNhan benhNhan)
        {
            if (ChiTietDanhSach == null)
            {
                ChiTietDanhSach = new List<ChiTietDanhSachKham>();
            }
            var chiTiet = new ChiTietDanhSachKham
            {
                STT = SoLuongHienTai + 1,
                TrangThai = "Chờ khám",
                BenhNhan = benhNhan
            };
            ChiTietDanhSach.Add(chiTiet);
            return chiTiet;
        }

        public void XoaBenhNhan(BenhNhan benhNhan)
        {
           var chiTiet = ChiTietDanhSach?.Find(ct => ct.BenhNhan.MaBenhNhan == benhNhan.MaBenhNhan);
            if (chiTiet != null)
            {
                ChiTietDanhSach.Remove(chiTiet);
                // Cập nhật lại STT sau khi xóa
                for (int i = 0; i < ChiTietDanhSach.Count; i++)
                {
                    ChiTietDanhSach[i].STT = i + 1;
                }
            }
        }
    }


}
