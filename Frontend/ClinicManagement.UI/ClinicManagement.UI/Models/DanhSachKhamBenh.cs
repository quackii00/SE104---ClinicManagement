using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.Models
{
    public class ChiTietDanhSachKham
    {
        public int STT { get; set; }
        public string TrangThai { get; set; } = "Chờ khám";
        public BenhNhan BenhNhan { get; set; } = new BenhNhan();

        // 🌟 ĐÃ THÊM: Thuộc tính này để hứng mã phiếu khám từ ChiTietKhamItemDto của Backend gửi về
        public string? MaPhieuKham { get; set; }
    }

    public class DanhSachKhamBenh
    {
        public int Id { get; set; }
        public DateTime NgayKham { get; set; } = DateTime.Today;

        // 🌟 ĐÃ THÊM: Thuộc tính này hứng cấu hình SoBenhNhanToiDaNgay của Backend để hiển thị lên Dashboard
        public int SoBenhNhanToiDaNgay { get; set; } = 40;

        // 🌟 ĐÃ THÊM: Thuộc tính này hứng TongDoanhThuNgay từ Backend DTO
        public decimal TongDoanhThuNgay { get; set; }

        public int SoLuongHienTai => ChiTietDanhSach?.Count ?? 0;
        public List<ChiTietDanhSachKham> ChiTietDanhSach { get; set; } = new List<ChiTietDanhSachKham>();

        /// <summary>
        /// Kiểm tra xem danh sách đã đạt giới hạn tối đa chưa. 
        /// Sửa lại logic gán dấu '=' để chặn chính xác khi danh sách chạm mốc giới hạn.
        /// </summary>
        public bool KiemTraGioiHan(int soLuongToiDa)
        {
            return SoLuongHienTai < soLuongToiDa;
        }

        /// <summary>
        /// Thêm bệnh nhân mới vào danh sách khám và tự sinh STT tuần tự
        /// </summary>
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

        /// <summary>
        /// Xóa ca khám ra khỏi danh sách và tự động đánh lại STT từ 1 tránh bị đứt đoạn số thứ tự
        /// </summary>
        public void XoaBenhNhan(BenhNhan benhNhan)
        {
            if (ChiTietDanhSach == null || benhNhan == null) return;

            var chiTiet = ChiTietDanhSach.Find(ct => ct.BenhNhan.MaBenhNhan == benhNhan.MaBenhNhan);
            if (chiTiet != null)
            {
                ChiTietDanhSach.Remove(chiTiet);

                // Cập nhật lại STT liên tục không bị mất quãng (Ví dụ xóa STT 2 thì STT 3 thành STT 2)
                for (int i = 0; i < ChiTietDanhSach.Count; i++)
                {
                    ChiTietDanhSach[i].STT = i + 1;
                }
            }
        }
    }
}