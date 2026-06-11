using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Service kết nối với TraCuuController của Backend (Mọi vai trò đều dùng được)
    /// </summary>
    public class TraCuuService : BaseApiService
    {
        public TraCuuService() : base()
        {
            // Tái sử dụng HttpClient mang tên 'Client' từ BaseApiService
        }

        /// <summary>
        /// GET: api/tracuu/benhnhan?hoTen=...&namSinh=...&gioiTinh=...&ngayKham=yyyy-MM-dd
        /// Tra cứu nâng cao danh sách bệnh nhân dựa theo các bộ lọc
        /// </summary>
        /// <summary>
        /// GET: api/tracuu/benhnhan?hoTen=...&namSinh=...&gioiTinh=...&ngayKham=yyyy-MM-dd
        /// 🌟 ĐÃ KHỚP CỨNG BACKEND: Chỉ truyền đúng 4 tham số qua query string
        /// </summary>
        public async Task<List<TraCuuBenhNhanResultDto>> TraCuuBenhNhanAsync(string? hoTen, int? namSinh, string? gioiTinh, DateTime? ngayKham)
        {
            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(hoTen))
                    queryParams.Add($"hoTen={Uri.EscapeDataString(hoTen)}");

                if (namSinh.HasValue)
                    queryParams.Add($"namSinh={namSinh.Value}");

                if (!string.IsNullOrWhiteSpace(gioiTinh))
                    queryParams.Add($"gioiTinh={Uri.EscapeDataString(gioiTinh)}");

                if (ngayKham.HasValue)
                {
                    string formattedDate = ngayKham.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    queryParams.Add($"ngayKham={formattedDate}");
                }

                string url = "tracuu/benhnhan";
                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }

                return await GetAsync<List<TraCuuBenhNhanResultDto>>(url) ?? new List<TraCuuBenhNhanResultDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TraCuuService] Lỗi TraCuuBenhNhanAsync: {ex.Message}");
                return new List<TraCuuBenhNhanResultDto>();
            }
        }

        /// <summary>
        /// GET: api/tracuu/benhnhan/{maBenhNhan}/lichsu
        /// Lấy toàn bộ lịch sử các ca khám, toa thuốc của 1 bệnh nhân cụ thể
        /// </summary>
        public async Task<List<LichSuKhamDto>> GetLichSuKhamAsync(string maBenhNhan)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maBenhNhan)) return new List<LichSuKhamDto>();

                string url = $"tracuu/benhnhan/{Uri.EscapeDataString(maBenhNhan)}/lichsu";
                return await GetAsync<List<LichSuKhamDto>>(url) ?? new List<LichSuKhamDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TraCuuService] Lỗi GetLichSuKhamAsync: {ex.Message}");
                return new List<LichSuKhamDto>();
            }
        }
    }
}