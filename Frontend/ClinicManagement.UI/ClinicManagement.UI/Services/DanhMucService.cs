using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    public class DanhMucService : BaseApiService
    {
        public DanhMucService() : base() { }

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