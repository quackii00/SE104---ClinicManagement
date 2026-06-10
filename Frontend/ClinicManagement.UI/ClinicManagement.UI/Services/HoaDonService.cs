
using ClinicManagement.UI.DTOs;
using System;
using System.Threading.Tasks;

namespace ClinicManagement.UI.Services
{
    public class HoaDonService : BaseApiService
    {
        public async Task<HoaDonDto?> PreviewHoaDonAsync(string maPhieuKham)
        {
            string endpoint = $"hoadon/preview?maPhieuKham={Uri.EscapeDataString(maPhieuKham)}";
            return await GetAsync<HoaDonDto>(endpoint);
        }

        public async Task<HoaDonDto?> CreateHoaDonAsync(string maPhieuKham)
        {
            var actualRequest = new { MaPhieuKham = maPhieuKham };
            return await PostAsync<object, HoaDonDto>("hoadon", actualRequest);
        }
    }
}