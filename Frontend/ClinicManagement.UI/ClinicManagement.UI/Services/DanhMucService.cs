using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Service công khai dùng chung để các vai trò (Bác sĩ, Tiếp tân, Kế toán, Admin) 
    /// có thể GET lấy danh sách dữ liệu hiển thị lên ComboBox/ListView.
    /// </summary>
    public class DanhMucService : BaseApiService
    {
        public DanhMucService() : base() { }

        // ==================== GET METHODS (api/danhmuc/...) ====================

        /// <summary>
        /// GET: api/danhmuc/loaibenh
        /// </summary>
        public async Task<List<LoaiBenhDto>> GetLoaiBenhAsync()
        {
            try
            {
                return await GetAsync<List<LoaiBenhDto>>("danhmuc/loaibenh") ?? new List<LoaiBenhDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhMucService] Lỗi GetLoaiBenh: {ex.Message}");
                return new List<LoaiBenhDto>();
            }
        }

        /// <summary>
        /// GET: api/danhmuc/thuoc
        /// </summary>
        public async Task<List<ThuocDto>> GetThuocAsync()
        {
            try
            {
                return await GetAsync<List<ThuocDto>>("danhmuc/thuoc") ?? new List<ThuocDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhMucService] Lỗi GetThuoc: {ex.Message}");
                return new List<ThuocDto>();
            }
        }

        /// <summary>
        /// 🌟 BỔ SUNG ĐỂ SỬA LỖI COMPILE: GET: api/danhmuc/donvi
        /// </summary>
        public async Task<List<DonViDto>> GetDonViAsync()
        {
            try
            {
                return await GetAsync<List<DonViDto>>("danhmuc/donvi") ?? new List<DonViDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhMucService] Lỗi GetDonVi: {ex.Message}");
                return new List<DonViDto>();
            }
        }

        /// <summary>
        /// GET: api/danhmuc/cachdung
        /// </summary>
        public async Task<List<CachDungDto>> GetCachDungAsync()
        {
            try
            {
                return await GetAsync<List<CachDungDto>>("danhmuc/cachdung") ?? new List<CachDungDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DanhMucService] Lỗi GetCachDung: {ex.Message}");
                return new List<CachDungDto>();
            }
        }
    }
}