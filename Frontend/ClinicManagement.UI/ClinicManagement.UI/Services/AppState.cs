using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClinicManagement.UI.Models;

namespace ClinicManagement.UI.Services
{
    public class AppState : INotifyPropertyChanged
    {
        private static AppState _instance;
        public static AppState Instance => _instance ??= new AppState();

        public event PropertyChangedEventHandler PropertyChanged;

        private DanhSachKhamBenh _danhSachKhamHienTai;
        public DanhSachKhamBenh DanhSachKhamHienTai
        {
            get => _danhSachKhamHienTai;
            set { _danhSachKhamHienTai = value; OnPropertyChanged(); }
        }

        private int _soLuongToiDaHeThong = 40;
        public int SoLuongToiDaHeThong
        {
            get => _soLuongToiDaHeThong;
            set { _soLuongToiDaHeThong = value; OnPropertyChanged(); }
        }

        // 🌟 Tiền khám hệ thống (QĐ4) – để các màn hình khác cập nhật ngay khi Admin đổi quy định,
        // tương tự cơ chế của SoLuongToiDaHeThong (QĐ1).
        private decimal _tienKhamHeThong = 30000m;
        public decimal TienKhamHeThong
        {
            get => _tienKhamHeThong;
            set { _tienKhamHeThong = value; OnPropertyChanged(); }
        }

        private decimal _tongDoanhThuTrongNgay = 0;
        public decimal TongDoanhThuTrongNgay
        {
            get => _tongDoanhThuTrongNgay;
            set { _tongDoanhThuTrongNgay = value; OnPropertyChanged(); }
        }

        // --- THÔNG TIN AUTHENTICATION ---
        public string AuthToken { get; set; }

        // 🌟 Thêm dòng này để ánh xạ mượt mà với code gọi ở các Service cũ
        public string CurrentToken => AuthToken;

        public string CurrentUserName { get; set; }
        public string CurrentUserRole { get; set; }
        public string CurrentUserRoleCode { get; set; }
        public string LastUsedEmail { get; set; }
        public string LastUsedRole { get; set; }

        public void TriggerDashboardUpdate()
        {
            OnPropertyChanged(nameof(DanhSachKhamHienTai));
            OnPropertyChanged(nameof(TongDoanhThuTrongNgay));
        }

        public void Reset()
        {
            AuthToken = null;
            CurrentUserName = null;
            CurrentUserRole = null;
            CurrentUserRoleCode = null;
            DanhSachKhamHienTai = null;
            TongDoanhThuTrongNgay = 0;
            // Xóa luôn quy định đã nạp để không "rò rỉ" giá trị cũ sang phiên đăng nhập khác.
            SoLuongToiDaHeThong = 40;
            TienKhamHeThong = 30000m;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}