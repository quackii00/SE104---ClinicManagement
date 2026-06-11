using System;

namespace ClinicManagement.UI.DTOs
{
    /// <summary>
    /// Cấu hình tham số hệ thống + Thống kê số lượng danh mục đầu ngày (QĐ1, QĐ2, QĐ4)
    /// </summary>
    public class ThamSoDto
    {
        public int SoBenhNhanToiDaNgay { get; set; }
        public decimal TienKham { get; set; }

        // Thống kê số lượng từ Backend (Chỉ đọc để hiển thị lên thẻ UI)
        public int SoLoaiBenh { get; set; }
        public int SoLoaiThuoc { get; set; }
        public int SoDonVi { get; set; }
        public int SoCachDung { get; set; }
    }

    /// <summary>
    /// Gói dữ liệu gửi lên khi Admin bấm nút "Lưu thay đổi" quy định 1 và 4
    /// </summary>
    public class UpdateThamSoRequest
    {
        public int SoBenhNhanToiDaNgay { get; set; }
        public decimal TienKham { get; set; }

        // Bổ sung thêm 3 danh sách này để nhận dữ liệu từ WPF gửi lên
        public List<string> DanhSachLoaiBenh { get; set; } = new();
        public List<string> DanhSachLoaiThuoc { get; set; } = new();
        public List<string> DanhSachCachDung { get; set; } = new();
    }

    /// <summary>
    /// Hứng chuỗi thông điệp phản hồi từ các lệnh xóa hoặc báo lỗi của Server
    /// </summary>
    public class MessageResponse
    {
        public string Message { get; set; } = string.Empty;

        public MessageResponse() { }
        public MessageResponse(string message) => Message = message;
    }
}