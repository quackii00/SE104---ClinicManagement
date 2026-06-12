using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly QuyDinhService _quyDinhService;

        private string _todayPatients = "0/40";
        private int _examinationFormsCount = 0;
        private decimal _totalRevenue = 0;
        private decimal _tienKhamCoDinh = 0; // Cấu hình tiền khám nạp từ quy định Backend
        private bool _isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        // --- CÁC THUỘC TÍNH BINDING RA GIAO DIỆN XAML ---
        public string TodayPatients { get => _todayPatients; set { _todayPatients = value; OnPropertyChanged(); } }
        public int ExaminationFormsCount { get => _examinationFormsCount; set { _examinationFormsCount = value; OnPropertyChanged(); } }
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged(); } }
        public decimal TienKhamCoDinh { get => _tienKhamCoDinh; set { _tienKhamCoDinh = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        public DashboardViewModel()
        {
            _quyDinhService = new QuyDinhService();

            // Đăng ký lắng nghe sự kiện thay đổi dữ liệu từ AppState Singleton toàn cục
            AppState.Instance.PropertyChanged += HandleAppStateChanged;

            // 🌟 LUỒNG TỰ ĐỘNG: Nạp song song cả dữ liệu danh sách khám lẫn quy định tham số từ Server ngầm
            _ = InitializeDashboardDataAsync();
        }

        /// <summary>
        /// Khởi tạo dữ liệu bất đồng bộ cho Dashboard
        /// </summary>
        private async Task InitializeDashboardDataAsync()
        {
            try
            {
                IsLoading = true;

                // Gọi API lấy tham số quy định hệ thống (QĐ1 / QĐ4) từ Server
                var thamSo = await _quyDinhService.GetThamSoAsync();
                if (thamSo != null)
                {
                    // Lưu vào két sắt chung để các màn hình khác (như UpdateRegulations) dùng ké
                    AppState.Instance.SoLuongToiDaHeThong = thamSo.SoBenhNhanToiDaNgay;
                    AppState.Instance.TienKhamHeThong = thamSo.TienKham;
                    TienKhamCoDinh = thamSo.TienKham;
                }

                // Sau khi nạp quy định xong, tiến hành tính toán hiển thị lên UI
                RefreshDashboardUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Lỗi nạp quy định đầu ngày: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Bẫy sự kiện thay đổi trạng thái từ bộ nhớ RAM chung
        /// </summary>
        private void HandleAppStateChanged(object sender, PropertyChangedEventArgs e)
        {
            // Tự động làm mới số liệu Dashboard nếu danh sách khám hoặc tổng doanh thu phát sinh biến động
            if (e.PropertyName == nameof(AppState.Instance.DanhSachKhamHienTai) ||
                e.PropertyName == nameof(AppState.Instance.TongDoanhThuTrongNgay) ||
                e.PropertyName == nameof(AppState.Instance.SoLuongToiDaHeThong))
            {
                RefreshDashboardUI();
            }
            // Tiền khám đổi (Admin lưu QĐ4) -> cập nhật ngay thẻ trên Dashboard
            else if (e.PropertyName == nameof(AppState.Instance.TienKhamHeThong))
            {
                TienKhamCoDinh = AppState.Instance.TienKhamHeThong;
            }
        }

        /// <summary>
        /// Đồng bộ số liệu thực tế từ AppState lên UI
        /// </summary>
        private void RefreshDashboardUI()
        {
            try
            {
                var dsKham = AppState.Instance.DanhSachKhamHienTai;

                // Lấy con số giới hạn hệ thống vừa nạp (mặc định là 40 nếu chưa kết nối được API)
                int soToiDa = AppState.Instance.SoLuongToiDaHeThong > 0 ? AppState.Instance.SoLuongToiDaHeThong : 40;

                if (dsKham != null && dsKham.ChiTietDanhSach != null)
                {
                    int soLuongHienTai = dsKham.ChiTietDanhSach.Count;
                    TodayPatients = $"{soLuongHienTai}/{soToiDa}";

                    // Đếm tổng số ca đã lập phiếu khám bệnh (Có trạng thái "Đã khám" hoặc có MaPhieuKham)
                    ExaminationFormsCount = dsKham.ChiTietDanhSach.Count(p =>
                        p.TrangThai == "Đã khám" || !string.IsNullOrEmpty(p.MaPhieuKham));
                }
                else
                {
                    // Nếu chưa có danh sách ca khám, vẫn hiện đúng định dạng "0/giới hạn" chuẩn chỉnh
                    TodayPatients = $"0/{soToiDa}";
                    ExaminationFormsCount = 0;
                }

                // Đồng bộ tổng doanh thu trong ngày
                TotalRevenue = AppState.Instance.TongDoanhThuTrongNgay;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Lỗi cập nhật giao diện: {ex.Message}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            // Bảo vệ luồng cập nhật UI an toàn trên Main Thread của WPF
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }

        /// <summary>
        /// Giải phóng sự kiện tránh Memory Leak (Rò rỉ RAM) khi user đóng tab Dashboard sang màn hình khác
        /// </summary>
        public void Dispose()
        {
            AppState.Instance.PropertyChanged -= HandleAppStateChanged;
        }
    }
}