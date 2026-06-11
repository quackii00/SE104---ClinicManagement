using System.Threading.Tasks;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// YC6 – Service gọi API nhóm "Cập nhật quy định".
    /// BẮT BUỘC kế thừa BaseApiService để dùng chung HttpClient + tự đính kèm JWT.
    /// Trỏ tới controller Backend: [Route("api/quydinh")] (yêu cầu quyền Admin).
    /// </summary>
    public class QuyDinhService : BaseApiService
    {
        // BaseAddress đã là ".../api/" nên endpoint chỉ cần phần đuôi "quydinh".
        private const string Endpoint = "quydinh";

        /// <summary>
        /// Lấy bộ quy định hiện tại từ Server để đổ lên form (GET api/quydinh).
        /// Dùng khi mở màn hình "Cập nhật quy định" để hiển thị giá trị đang áp dụng.
        /// </summary>
        public async Task<ThamSoDto> GetThamSoAsync()
        {
            return await GetAsync<ThamSoDto>(Endpoint);
        }

        /// <summary>
        /// Gửi dữ liệu CẬP NHẬT quy định lên Server bằng phương thức PUT (PUT api/quydinh).
        /// Đây là hàm xử lý cho sự kiện bấm nút "Cập nhật" trên màn hình UpdateRegulations.
        /// Trả về bộ quy định mới nhất sau khi Server đã lưu vào CSDL.
        /// </summary>
        public async Task<ThamSoDto> CapNhatQuyDinhAsync(UpdateThamSoRequest request)
        {
            return await PutAsync<UpdateThamSoRequest, ThamSoDto>(Endpoint, request);
        }
    }
}
