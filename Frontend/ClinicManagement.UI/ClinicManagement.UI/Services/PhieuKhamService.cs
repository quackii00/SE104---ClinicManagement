using System;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    public class PhieuKhamService : BaseApiService
    {
        public PhieuKhamService() : base()
        {
            // Thừa hưởng biến kết nối 'Client' từ BaseApiService
        }

        /// <summary>
        /// POST: api/phieukham
        /// Gửi yêu cầu lập phiếu khám mới lên Server (Chỉ dành cho Bác sĩ / Admin)
        /// </summary>
        public async Task<PhieuKhamDto?> CreatePhieuKhamAsync(CreatePhieuKhamRequest request)
        {
            try
            {
                // Gọi tới endpoint: api/phieukham
                return await PostAsync<CreatePhieuKhamRequest, PhieuKhamDto>("phieukham", request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PhieuKhamService] Lỗi CreatePhieuKhamAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 🌟 HÀM BỔ SUNG CHUẨN BACKEND: GET api/phieukham/{maPhieuKham}
        /// Bốc thông tin chi tiết của một phiếu khám cũ kèm theo đơn thuốc đã kê
        /// </summary>
        public async Task<PhieuKhamDto?> GetPhieuKhamByIdAsync(string maPhieuKham)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maPhieuKham)) return null;

                // Chuẩn hóa endpoint dạng: phieukham/PK000001
                string endpoint = $"phieukham/{Uri.EscapeDataString(maPhieuKham)}";

                System.Diagnostics.Debug.WriteLine($"[PhieuKhamService] Đang bốc chi tiết phiếu khám cũ: {endpoint}");
                return await GetAsync<PhieuKhamDto>(endpoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PhieuKhamService] Lỗi GetPhieuKhamByIdAsync: {ex.Message}");
                return null;
            }
        }
    }
}