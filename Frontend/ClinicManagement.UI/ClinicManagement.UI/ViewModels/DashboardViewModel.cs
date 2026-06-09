using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services; // Gọi AppState dùng chung

namespace ClinicManagement.UI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private string _todayPatients;
        private int _examinationFormsCount;
        private decimal _totalRevenue;
        private bool _isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        public string TodayPatients { get => _todayPatients; set { _todayPatients = value; OnPropertyChanged(); } }
        public int ExaminationFormsCount { get => _examinationFormsCount; set { _examinationFormsCount = value; OnPropertyChanged(); } }
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        public DashboardViewModel()
        {
            TodayPatients = "0/--";
            ExaminationFormsCount = 0;
            TotalRevenue = 0;

            // 🌟 ĐỒNG BỘ THỜI GIAN THỰC: Đăng ký lắng nghe sự kiện thay đổi từ "két sắt" AppState trung tâm
            // Hễ form Tiếp nhận thêm người hoặc Hóa đơn tăng tiền, Dashboard sẽ tự động nhảy số lập tức!
            AppState.Instance.PropertyChanged += OnAppStatePropertyChanged;

            // Nạp dữ liệu nóng trực tiếp từ AppState lên UI ngay khi vừa mở màn hình
            RefreshDashboardUI();
        }

        /// <summary>
        /// Bộ bắt sóng sự kiện: Tự động chạy khi kho dữ liệu AppState có biến động tăng/giảm
        /// </summary>
        private void OnAppStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Bắt các tín hiệu thay đổi danh sách khám hoặc tổng doanh thu
            if (e.PropertyName == nameof(AppState.Instance.DanhSachKhamHienTai) ||
                e.PropertyName == nameof(AppState.Instance.SoLuongToiDaHeThong) ||
                e.PropertyName == nameof(AppState.Instance.TongDoanhThuTrongNgay))
            {
                // Ép Dashboard tính toán lại các con số hiển thị
                RefreshDashboardUI();
            }
        }

        /// <summary>
        /// Hàm bốc dữ liệu trực tiếp từ AppState để ép giao diện vẽ lại các vòng tròn thống kê
        /// </summary>
        private void RefreshDashboardUI()
        {
            var danhSachGoc = AppState.Instance.DanhSachKhamHienTai;
            int maxHeThong = AppState.Instance.SoLuongToiDaHeThong;
            decimal doanhThuGoc = AppState.Instance.TongDoanhThuTrongNgay;

            if (danhSachGoc != null)
            {
                // 1. Đồng bộ số lượng bệnh nhân khám trong ngày (Ví dụ: 3/40)
                TodayPatients = $"{danhSachGoc.SoLuongHienTai}/{maxHeThong}";

                // 2. Tính toán số phiếu khám đã lập dựa trên dữ liệu thật đang có trên RAM
                int soPhieuKhamDaLap = 0;
                if (danhSachGoc.ChiTietDanhSach != null)
                {
                    foreach (var item in danhSachGoc.ChiTietDanhSach)
                    {
                        // Giả lập đếm: Nếu trạng thái là "Đã khám" hoặc "Đang khám" coi như đã lập phiếu
                        if (item.TrangThai == "Đã khám" || item.TrangThai == "Đang khám")
                        {
                            soPhieuKhamDaLap++;
                        }
                    }
                }
                ExaminationFormsCount = soPhieuKhamDaLap;

                // 3. Đồng bộ tổng doanh thu nóng từ kho dùng chung
                TotalRevenue = doanhThuGoc;
            }
        }

        /// <summary>
        /// Hàm này giữ lại để không bị lỗi cấu trúc cũ, nhưng ruột sẽ đọc đồng bộ từ AppState
        /// </summary>
        public async Task LoadDashboardDataAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                await Task.Delay(300); // Giả lập độ trễ mạng cực nhẹ
                RefreshDashboardUI();
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}