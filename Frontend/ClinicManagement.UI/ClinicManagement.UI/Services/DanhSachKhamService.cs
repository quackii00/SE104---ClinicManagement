using ClinicManagement.UI.DTOs;
using System;
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClinicManagement.UI.Services
{
    public class DanhSachKhamService : BaseApiService
    {
        public DanhSachKhamService() : base() { }

        /// <summary>
        /// Gọi API GET: api/danhsachkham/today để lấy danh sách hôm nay
        /// </summary>
        public async Task<DanhSachKhamDto> GetTodayPatientsAsync()
        {
            try
            {
                return await GetAsync<DanhSachKhamDto>("danhsachkham/today");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhSachKhamService] Lỗi GetTodayPatientsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 🌟 HÀM MỚI BỔ SUNG: Gọi API GET: api/danhsachkham?ngay=yyyy-MM-dd
        /// Chuẩn hóa định dạng chuỗi ngày gửi lên Query string để Server không parse lỗi
        /// </summary>
        public async Task<DanhSachKhamDto> GetPatientsByDateAsync(DateTime date)
        {
            try
            {
                // Ép định dạng bắt buộc yyyy-MM-dd (Ví dụ: 2026-06-11) gửi qua URL
                string formattedDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return await GetAsync<DanhSachKhamDto>($"danhsachkham?ngay={formattedDate}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhSachKhamService] Lỗi GetPatientsByDateAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gọi API POST: api/danhsachkham/tiepnhan
        /// </summary>
        public async Task<ChiTietKhamItemDto> TiepNhanBenhNhanAsync(DangKyKhamRequest request)
        {
            try
            {
                // Đảm bảo trường NgayKham trong request trước khi bắn JSON đi cũng được gửi đúng dạng (xử lý ở ViewModel)
                return await PostAsync<DangKyKhamRequest, ChiTietKhamItemDto>("danhsachkham/tiepnhan", request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhSachKhamService] Lỗi TiepNhanBenhNhanAsync: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// POST: api/danhsachkham/dangkham/{maBenhNhan}
        /// Đánh dấu bệnh nhân "Đang khám" khi bác sĩ mở phiếu khám (Chờ khám -> Đang khám).
        /// </summary>
        public async Task BatDauKhamAsync(string maBenhNhan)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maBenhNhan)) return;
                await PostAsync<object, MessageResponse>($"danhsachkham/dangkham/{Uri.EscapeDataString(maBenhNhan)}", new { });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhSachKhamService] Lỗi BatDauKhamAsync: {ex.Message}");
            }
        }
    }
}