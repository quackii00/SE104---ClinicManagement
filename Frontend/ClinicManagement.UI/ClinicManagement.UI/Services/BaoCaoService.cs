using System;
using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    public class BaoCaoService : BaseApiService
    {
        public async Task<BaoCaoDoanhThuDto?> GetMonthlyRevenueReportAsync(int thang, int nam)
        {
            string endpoint = $"baocao/doanhthu?thang={thang}&nam={nam}";
            return await GetAsync<BaoCaoDoanhThuDto>(endpoint);
        }

        public async Task<BaoCaoSuDungThuocDto?> GetMedicineUsageReportAsync(int thang, int nam)
        {
            string endpoint = $"baocao/sudungthuoc?thang={thang}&nam={nam}";
            return await GetAsync<BaoCaoSuDungThuocDto>(endpoint);
        }
    }
}