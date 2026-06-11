using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.ViewModels;

namespace ClinicManagement.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly DanhSachKhamService _danhSachKhamService;
        private string _userRole;
        private string _userName;
        private object _currentView;
        private bool _isDataLoaded;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserRole { get => _userRole; set { _userRole = value; OnPropertyChanged(); } }
        public string UserName { get => _userName; set { _userName = value; OnPropertyChanged(); } }
        public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(); } }
        public bool IsDataLoaded { get => _isDataLoaded; set { _isDataLoaded = value; OnPropertyChanged(); } }

        public ICommand NavigationCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainWindowViewModel()
        {
            _danhSachKhamService = new DanhSachKhamService();
            UserRole = AppState.Instance.CurrentUserRole;
            UserName = AppState.Instance.CurrentUserName;

            NavigationCommand = new RelayCommand(p => Navigate(p?.ToString()));
            LogoutCommand = new RelayCommand(p => Logout());

            CurrentView = new DashboardViewModel();
            _ = InitializeApplicationDataAsync();
        }

        private async Task InitializeApplicationDataAsync()
        {
            await LoadAllDailyDataFromServerAsync();
            Navigate("Dashboard");
        }

        private async Task LoadAllDailyDataFromServerAsync()
        {
            try
            {
                IsDataLoaded = false;
                var responseData = await _danhSachKhamService.GetTodayPatientsAsync();

                var danhSachModel = new DanhSachKhamBenh
                {
                    NgayKham = responseData?.NgayKham ?? DateTime.Today,
                    ChiTietDanhSach = new List<ChiTietDanhSachKham>()
                };

                if (responseData?.ChiTietDanhSach != null)
                {
                    foreach (var item in responseData.ChiTietDanhSach)
                    {
                        danhSachModel.ChiTietDanhSach.Add(new ChiTietDanhSachKham
                        {
                            STT = item.STT,
                            TrangThai = item.TrangThai,
                            MaPhieuKham = item.MaPhieuKham,
                            BenhNhan = new BenhNhan { MaBenhNhan = item.MaBenhNhan, HoTen = item.HoTen, GioiTinh = item.GioiTinh, NamSinh = item.NamSinh, DiaChi = item.DiaChi }
                        });
                    }
                    AppState.Instance.SoLuongToiDaHeThong = responseData.SoBenhNhanToiDaNgay;
                    AppState.Instance.TongDoanhThuTrongNgay = responseData.TongDoanhThuNgay;
                }
                AppState.Instance.DanhSachKhamHienTai = danhSachModel;
                IsDataLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp dữ liệu: {ex.Message}");
            }
        }

        public void Navigate(string targetView, object parameter = null)
        {
            if (string.IsNullOrEmpty(targetView)) return;

            switch (targetView)
            {
                case "Dashboard":
                    CurrentView = new DashboardViewModel();
                    break;
                case "PatientList":
                    CurrentView = new PatientListViewModel(this);
                    break;
                case "PatientLookup":
                    CurrentView = new PatientLookupViewModel(this);
                    break;
                case "Invoice":
                    if (parameter is ChiTietDanhSachKham patientContext)
                        CurrentView = new InvoiceViewModel(this, patientContext);
                    else
                        CurrentView = new PatientListViewModel(this);
                    break;
                case "MedicalRecord":
                    if (parameter is MedicalRecordViewModel mrvm)
                        CurrentView = mrvm;
                    break;
                // BẠN THIẾU ĐOẠN NÀY NÊN NÓ MỚI NHẢY VỀ DASHBOARD:
                case "Report":
                    CurrentView = new ReportViewModel(); // Hoặc ViewModel báo cáo của bạn
                    break;
                
                default:
                    CurrentView = new DashboardViewModel();
                    break;
            }
        }
        private void Logout()
        {
            // 1. Xóa sạch dữ liệu phiên
            new TokenStorageService().ClearToken();
            AppState.Instance.Reset();

            // 2. Tạo cửa sổ đăng nhập mới
            var loginWindow = new LoginWindow();

            // 3. Đặt nó làm cửa sổ chính của ứng dụng trước khi đóng màn hình cũ
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            // 4. Đóng cửa sổ hiện tại (MainWindow)
            foreach (Window window in Application.Current.Windows)
            {
                if (window != loginWindow)
                {
                    window.Close();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}