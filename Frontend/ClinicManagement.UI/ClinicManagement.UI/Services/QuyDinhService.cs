using ClinicManagement.UI.DTOs;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Service tương tác với QuyDinhController của API (Yêu cầu vai trò Admin)
    /// </summary>
    public class QuyDinhService : BaseApiService
    {
        public QuyDinhService() : base()
        {
            // Tái sử dụng HttpClient cấu hình sẵn từ lớp cha BaseApiService
        }

        // ==========================================
        // 1. THAM SỐ & QUY ĐỊNH HỆ THỐNG (QĐ1 / QĐ4 / THỐNG KÊ QĐ2)
        // ==========================================

        /// <summary>
        /// GET: api/quydinh
        /// Lấy tham số hệ thống kèm theo số lượng thống kê các danh mục
        /// </summary>
        public async Task<ThamSoDto?> GetThamSoAsync()
        {
            try
            {
                // Dùng endpoint "thamso" (mọi vai trò đọc được) thay vì "quydinh" (chỉ Admin),
                // để Dashboard của Bác sĩ / Tiếp tân / Kế toán cũng lấy được tiền khám + số BN tối đa.
                return await GetAsync<ThamSoDto>("thamso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi GetThamSo: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// PUT: api/quydinh
        /// Cập nhật số bệnh nhân tối đa trong ngày và tiền khám cố định
        /// </summary>
        public async Task<ThamSoDto?> UpdateThamSoAsync(UpdateThamSoRequest request)
        {
            try
            {
                return await PutAsync<UpdateThamSoRequest, ThamSoDto>("quydinh", request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi UpdateThamSo: {ex.Message}");
                return null;
            }
        }

        // ==========================================
        // 2. DANH MỤC LOẠI BỆNH (QĐ2)
        // ==========================================

        /// <summary>
        /// POST: api/quydinh/loaibenh
        /// </summary>
        public async Task<LoaiBenhDto?> AddLoaiBenhAsync(UpsertLoaiBenhRequest request)
        {
            try
            {
                return await PostAsync<UpsertLoaiBenhRequest, LoaiBenhDto>("quydinh/loaibenh", request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi AddLoaiBenh: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// PUT: api/quydinh/loaibenh/{id}
        /// </summary>
        public async Task<LoaiBenhDto?> UpdateLoaiBenhAsync(int id, UpsertLoaiBenhRequest request)
        {
            try
            {
                return await PutAsync<UpsertLoaiBenhRequest, LoaiBenhDto>($"quydinh/loaibenh/{id}", request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi UpdateLoaiBenh: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// DELETE: api/quydinh/loaibenh/{id}
        /// Trả về (thành công?, thông điệp) – lấy đúng lý do từ Server nếu bị chặn xóa.
        /// </summary>
        public Task<(bool Success, string Message)> DeleteLoaiBenhAsync(int id)
            => DeleteWithMessageAsync($"quydinh/loaibenh/{id}", "Đã xóa loại bệnh.");

        // ==========================================
        // 3. DANH MỤC THUỐC (QĐ2 / QĐ4)
        // ==========================================

        /// <summary>
        /// POST: api/quydinh/thuoc
        /// </summary>
        public async Task<ThuocDto?> AddThuocAsync(UpsertThuocRequest request)
        {
            try
            {
                return await PostAsync<UpsertThuocRequest, ThuocDto>("quydinh/thuoc", request);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi AddThuoc: {ex.Message}"); return null; }
        }

        /// <summary>
        /// PUT: api/quydinh/thuoc/{id}
        /// </summary>
        public async Task<ThuocDto?> UpdateThuocAsync(int id, UpsertThuocRequest request)
        {
            try
            {
                return await PutAsync<UpsertThuocRequest, ThuocDto>($"quydinh/thuoc/{id}", request);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi UpdateThuoc: {ex.Message}"); return null; }
        }

        /// <summary>
        /// DELETE: api/quydinh/thuoc/{id}
        /// </summary>
        public Task<(bool Success, string Message)> DeleteThuocAsync(int id)
            => DeleteWithMessageAsync($"quydinh/thuoc/{id}", "Đã xóa thuốc.");

        // ==========================================
        // 4. DANH MỤC ĐƠN VỊ TÍNH (QĐ2)
        // ==========================================

        /// <summary>
        /// POST: api/quydinh/donvi
        /// </summary>
        public async Task<DonViDto?> AddDonViAsync(UpsertDonViRequest request)
        {
            try
            {
                return await PostAsync<UpsertDonViRequest, DonViDto>("quydinh/donvi", request);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi AddDonVi: {ex.Message}"); return null; }
        }

        /// <summary>
        /// PUT: api/quydinh/donvi/{id}
        /// </summary>
        public async Task<DonViDto?> UpdateDonViAsync(int id, UpsertDonViRequest request)
        {
            try
            {
                return await PutAsync<UpsertDonViRequest, DonViDto>($"quydinh/donvi/{id}", request);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi UpdateDonVi: {ex.Message}"); return null; }
        }

        /// <summary>
        /// DELETE: api/quydinh/donvi/{id}
        /// </summary>
        public Task<(bool Success, string Message)> DeleteDonViAsync(int id)
            => DeleteWithMessageAsync($"quydinh/donvi/{id}", "Đã xóa đơn vị tính.");

        // ==========================================
        // 5. DANH MỤC CÁCH DÙNG (QĐ2)
        // ==========================================

        /// <summary>
        /// POST: api/quydinh/cachdung
        /// </summary>
        public async Task<CachDungDto?> AddCachDungAsync(UpsertCachDungRequest request)
        {
            try
            {
                return await PostAsync<UpsertCachDungRequest, CachDungDto>("quydinh/cachdung", request);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi AddCachDung: {ex.Message}"); return null; }
        }

        /// <summary>
        /// PUT: api/quydinh/cachdung/{id}
        /// </summary>
        public async Task<CachDungDto?> UpdateCachDungAsync(int id, UpsertCachDungRequest request)
        {
            try
            {
                return await PutAsync<UpsertCachDungRequest, CachDungDto>($"quydinh/cachdung/{id}", request);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[QuyDinhService] Lỗi UpdateCachDung: {ex.Message}"); return null; }
        }

        /// <summary>
        /// DELETE: api/quydinh/cachdung/{id}
        /// </summary>
        public Task<(bool Success, string Message)> DeleteCachDungAsync(int id)
            => DeleteWithMessageAsync($"quydinh/cachdung/{id}", "Đã xóa cách dùng.");
    }
}