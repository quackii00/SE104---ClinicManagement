namespace ClinicManagement.UI.DTOs
{
    /// <summary>
    /// YC6 – Tham số / quy định hệ thống nhận về từ Backend (GET api/quydinh).
    /// Tương ứng 1-1 với ThamSoDto bên Backend.
    /// QĐ1: SoBenhNhanToiDaNgay; QĐ4: TienKham; QĐ2: các số đếm danh mục (chỉ đọc).
    /// </summary>
    public class ThamSoDto
    {
        public int SoBenhNhanToiDaNgay { get; set; }
        public decimal TienKham { get; set; }

        // Thống kê QĐ2 (chỉ đọc) – do Backend đếm từ các bảng danh mục
        public int SoLoaiBenh { get; set; }
        public int SoLoaiThuoc { get; set; }
        public int SoDonVi { get; set; }
        public int SoCachDung { get; set; }
    }

    /// <summary>
    /// Gói dữ liệu gửi LÊN Backend để cập nhật quy định (PUT api/quydinh).
    /// Tương ứng 1-1 với UpdateThamSoRequest bên Backend.
    /// </summary>
    public class UpdateThamSoRequest
    {
        public int SoBenhNhanToiDaNgay { get; set; }
        public decimal TienKham { get; set; }
    }
}
