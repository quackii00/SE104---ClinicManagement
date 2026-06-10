using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;

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
            // Tạm thời hardcode vai trò và tên người dùng, sau này sẽ lấy từ AppState hoặc dịch vụ xác thực
            
           UserRole = AppState.Instance.CurrentUserRole;

            //UserRole = AppState.Instance.CurrentUserRole;
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

                if (responseData != null && responseData.ChiTietDanhSach != null)
                {
                    foreach (var item in responseData.ChiTietDanhSach)
                    {
                        danhSachModel.ChiTietDanhSach.Add(new ChiTietDanhSachKham
                        {
                            STT = item.STT,
                            TrangThai = item.TrangThai,
                            BenhNhan = new BenhNhan
                            {
                                MaBenhNhan = item.MaBenhNhan,
                                HoTen = item.HoTen,
                                GioiTinh = item.GioiTinh,
                                NamSinh = item.NamSinh,
                                DiaChi = item.DiaChi
                            }
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
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp dữ liệu đầu ngày: {ex.Message}");
                AppState.Instance.DanhSachKhamHienTai = new DanhSachKhamBenh { NgayKham = DateTime.Today, ChiTietDanhSach = new List<ChiTietDanhSachKham>() };
            }
        }

        public void Navigate(string targetView)
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
                default:
                    CurrentView = new DashboardViewModel();
                    break;
            }
        }

        private void Logout()
        {
            AppState.Instance.Reset();
            Window activeMainWindow = null;

            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType().FullName == "ClinicManagement.UI.MainWindow" && window.IsVisible)
                {
                    activeMainWindow = window;
                    break;
                }
            }

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            activeMainWindow?.Close();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}