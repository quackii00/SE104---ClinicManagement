using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// YC5 – Service gọi API nhóm "Báo cáo tháng".
    /// Kế thừa BaseApiService để dùng chung HttpClient + tự đính kèm JWT.
    /// Trỏ tới controller Backend: [Route("api/baocao")] (yêu cầu quyền Kế Toán / Admin).
    /// </summary>
    public class BaoCaoService : BaseApiService
    {
        /// <summary>BM5.1 – Lấy báo cáo doanh thu theo tháng (GET api/baocao/doanhthu?thang=&nam=).</summary>
        public async Task<BaoCaoDoanhThuDto> GetDoanhThuAsync(int thang, int nam)
        {
            return await GetAsync<BaoCaoDoanhThuDto>($"baocao/doanhthu?thang={thang}&nam={nam}");
        }

        /// <summary>BM5.2 – Lấy báo cáo sử dụng thuốc theo tháng (GET api/baocao/sudungthuoc?thang=&nam=).</summary>
        public async Task<BaoCaoSuDungThuocDto> GetSuDungThuocAsync(int thang, int nam)
        {
            return await GetAsync<BaoCaoSuDungThuocDto>($"baocao/sudungthuoc?thang={thang}&nam={nam}");
        }
    }
}
