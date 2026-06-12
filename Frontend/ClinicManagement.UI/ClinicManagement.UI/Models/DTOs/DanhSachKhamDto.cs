using System;
using System.Collections.Generic;

namespace ClinicManagement.UI.DTOs
{
    public class DanhSachKhamDto
    {
        public int Id { get; set; }
        public DateTime NgayKham { get; set; }

        // Cấu hình lưu vết max bệnh nhân lấy từ bảng tham số hệ thống ngày hôm đó
        public int SoBenhNhanToiDaNgay { get; set; }

        // Danh sách chi tiết phẳng trả về từ thực thể CHITIETDANHSACH
        public List<ChiTietKhamItemDto> ChiTietDanhSach { get; set; } = new List<ChiTietKhamItemDto>();

        // Thêm trường doanh thu tổng hợp của ngày hôm đó từ thực thể HOADON
        public decimal TongDoanhThuNgay { get; set; }
    }

    public class ChiTietKhamItemDto
    {
        public int STT { get; set; }
        public string TrangThai { get; set; }
        public string MaBenhNhan { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public int NamSinh { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string MaPhieuKham { get; set; } // Dùng để đếm xem bao nhiêu người đã lập phiếu khám
    }
}