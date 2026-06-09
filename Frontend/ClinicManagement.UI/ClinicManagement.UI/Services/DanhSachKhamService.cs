using System;
using System.Collections.Generic;
using System.Linq; // BỔ SUNG: Để lọc dữ liệu bằng LINQ
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    public class DanhSachKhamService : BaseApiService
    {
        /// <summary>
        /// MOCK DATABASE: Kho dữ liệu gốc của toàn bộ bệnh nhân từng đến phòng khám
        /// </summary>
        private static readonly List<ChiTietKhamItemDto> _globalPatientDatabase = new List<ChiTietKhamItemDto>
        {
            new ChiTietKhamItemDto { MaBenhNhan = "BN001", HoTen = "Nguyễn Văn A", GioiTinh = "Nam", NamSinh = 1998, DiaChi = "Thủ Đức", TrangThai = "Đã khám", STT = 1 }
        };

        /// <summary>
        /// MOCK TABLE DANH SÁCH KHÁM: Nơi lưu vết những ai đã đăng ký khám NGÀY HÔM NAY
        /// </summary>
        private static readonly List<ChiTietKhamItemDto> _todayActivePatients = new List<ChiTietKhamItemDto>
        {
            _globalPatientDatabase[0] // Đầu ngày mặc định có sẵn ông Nguyễn Văn A
        };

        /// <summary>
        /// ĐỒNG BỘ CHUẨN: Lấy toàn bộ danh sách bệnh nhân thực tế đăng ký trong ngày hôm nay
        /// </summary>
        public async Task<DanhSachKhamDto> GetTodayPatientsAsync()
        {
            await Task.Delay(300);
            return new DanhSachKhamDto
            {
                Id = 101,
                NgayKham = DateTime.Today,
                SoBenhNhanToiDaNgay = 40,
                TongDoanhThuNgay = 1250000,
                // ĐÃ SỬA: Trả về danh sách động thực tế, không gán chết phần tử số 0 nữa
                ChiTietDanhSach = new List<ChiTietKhamItemDto>(_todayActivePatients)
            };
        }

        /// <summary>
        /// BIỂU ĐỒ TUẦN TỰ: timBenhNhan() - Tìm kiếm trong kho dữ liệu bệnh nhân hệ thống
        /// </summary>
        public async Task<ChiTietKhamItemDto> TimBenhNhanTheoSdtAsync(string sdt, string hoTen)
        {
            await Task.Delay(300);

            // Tìm kiếm trong kho dữ liệu tổng xem bệnh nhân này từng khám ở đây chưa
            var match = _globalPatientDatabase.FirstOrDefault(p =>
                (!string.IsNullOrEmpty(sdt) && sdt == "0909123456") ||
                p.HoTen.Equals(hoTen, StringComparison.OrdinalIgnoreCase));

            return match;
        }

        /// <summary>
        /// BIỂU ĐỒ TUẦN TỰ: taoBenhNhan() và themBenhNhan() vào danh sách khám ngày hôm nay
        /// </summary>
        public async Task<ChiTietKhamItemDto> TiepNhanBenhNhanAsync(DangKyKhamRequest request)
        {
            await Task.Delay(300);

            // STT tự động tăng dựa trên số lượng người thực tế đã đăng ký khám hôm nay
            int sttMoi = _todayActivePatients.Count + 1;

            var newPatient = new ChiTietKhamItemDto
            {
                STT = sttMoi,
                TrangThai = "Chờ khám",
                MaBenhNhan = "BN" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper(),
                HoTen = request.HoTen,
                GioiTinh = request.GioiTinh,
                NamSinh = request.NamSinh,
                DiaChi = request.DiaChi
            };

            // 1. Thêm vào kho dữ liệu hệ thống (taoBenhNhan)
            _globalPatientDatabase.Add(newPatient);

            // 2. Thêm vào danh sách ca khám hôm nay (themBenhNhan)
            _todayActivePatients.Add(newPatient);

            return newPatient;
        }
    }
}