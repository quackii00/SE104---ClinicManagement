using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClinicManagement.UI.Models;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Quản lý trạng thái toàn bộ ứng dụng (Kho lưu trữ trung tâm dùng chung trên RAM Client)
    /// ĐÃ NÂNG CẤP: Kế thừa INotifyPropertyChanged để đồng bộ tăng giảm tức thì lên toàn bộ UI
    /// </summary>
    public class AppState : INotifyPropertyChanged
    {
        private static AppState _instance;

        public static AppState Instance
        {
            get
            {
                _instance ??= new AppState();
                return _instance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // --- Biến private phục vụ luồng phát tín hiệu thông báo thay đổi ---
        private DanhSachKhamBenh _danhSachKhamHienTai;
        private int _soLuongToiDaHeThong = 40;
        private decimal _tongDoanhThuTrongNgay;
        private string _currentUserName;
        private string _currentUserRole;

        // --- CÁC THUỘC TÍNH ĐĂNG NHẬP SẴN CÓ ĐÃ ĐỒNG BỘ ---
        public string CurrentUserName
        {
            get => _currentUserName;
            set { _currentUserName = value; OnPropertyChanged(); }
        }
        public string CurrentUserRole
        {
            get => _currentUserRole;
            set { _currentUserRole = value; OnPropertyChanged(); }
        }
        public string CurrentUserEmail { get; set; }
        public string LastUsedEmail { get; set; }
        public string LastUsedRole { get; set; }


        // =========================================================================
        // --- BỔ SUNG: ĐỒNG BỘ DỮ LIỆU NGHIỆP VỤ TĂNG GIẢM THỜI GIAN THỰC ---
        // =========================================================================

        /// <summary>
        /// Khi danh sách này được gán mới (nạp đầu ngày hoặc làm mới), toàn app tự động vẽ lại
        /// </summary>
        public DanhSachKhamBenh DanhSachKhamHienTai
        {
            get => _danhSachKhamHienTai;
            set
            {
                _danhSachKhamHienTai = value;
                OnPropertyChanged();
                NotifyDataChanged(); // Phát tín hiệu dây chuyền
            }
        }

        /// <summary>
        /// Thay đổi giới hạn (Quy định 1) lập tức thay đổi các bộ đếm phần trăm trên UI
        /// </summary>
        public int SoLuongToiDaHeThong
        {
            get => _soLuongToiDaHeThong;
            set { _soLuongToiDaHeThong = value; OnPropertyChanged(); NotifyDataChanged(); }
        }

        /// <summary>
        /// Tiền tăng/giảm khi thanh toán hóa đơn lập tức nhảy số trên Dashboard
        /// </summary>
        public decimal TongDoanhThuTrongNgay
        {
            get => _tongDoanhThuTrongNgay;
            set { _tongDoanhThuTrongNgay = value; OnPropertyChanged(); }
        }

        private AppState()
        {
        }

        /// <summary>
        /// HÀM ĐẶC BIỆT: Ép các ViewModel con (Dashboard, PatientList) phải tự động 
        /// cập nhật lại bộ đếm CounterText hoặc biểu đồ khi có sự biến động tăng/giảm bệnh nhân.
        /// </summary>
        public void NotifyDataChanged()
        {
            OnPropertyChanged(nameof(DanhSachKhamHienTai));
            // Phát súng kích hoạt cho các hàm tính toán phụ thuộc lật trang tính lại dữ liệu thô
        }

        /// <summary>
        /// Hàm dọn sạch kho dữ liệu khi người dùng Đăng xuất
        /// </summary>
        public void Reset()
        {
            CurrentUserName = null;
            CurrentUserRole = null;
            CurrentUserEmail = null;
            DanhSachKhamHienTai = null;
            TongDoanhThuTrongNgay = 0;
        }

        public void SaveLastUsed()
        {
            LastUsedEmail = CurrentUserEmail;
            LastUsedRole = CurrentUserRole;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}