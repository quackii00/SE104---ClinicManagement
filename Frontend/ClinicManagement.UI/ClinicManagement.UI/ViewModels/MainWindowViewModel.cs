using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.Models;   // Gọi bộ Model của bạn
using ClinicManagement.UI.DTOs;     // Gọi DTO tổng hợp mới

// Khai báo chính xác các namespace của hệ thống
using ClinicManagement.UI.Views.UI.Dashboard;
using ClinicManagement.UI.Views.UI.Examination;
using ClinicManagement.UI.Views.UI.Patient;
using ClinicManagement.UI.Views.UI.Report;
using ClinicManagement.UI.Views.UI.Update;

namespace ClinicManagement.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly DanhSachKhamService _danhSachKhamService; // Khai báo service mạng

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
            // Khởi tạo lớp dịch vụ mạng kết nối Backend
            _danhSachKhamService = new DanhSachKhamService();

            UserRole = AppState.Instance.CurrentUserRole ?? "Admin";
            UserName = AppState.Instance.CurrentUserName ?? "Ngọc Huyền";

            NavigationCommand = new RelayCommand(p => Navigate(p?.ToString()));
            LogoutCommand = new RelayCommand(Logout);

            // KHỞI ĐỘNG LUỒNG: Gọi nạp dữ liệu thô từ Server ngầm ngay khi MainWindow vừa lên hình
            _ = InitializeApplicationDataAsync();
        }

        /// <summary>
        /// Hàm điều phối nạp dữ liệu từ Backend trước rồi mới mở Dashboard sau
        /// </summary>
        private async Task InitializeApplicationDataAsync()
        {
            await LoadAllDailyDataFromServerAsync();
            Navigate("Dashboard"); // Nạp xong xuôi mới mở Dashboard
        }

        /// <summary>
        /// Gọi API nhận dữ liệu đầu ngày từ Server nạp vào "két sắt" AppState
        /// </summary>
        private async Task LoadAllDailyDataFromServerAsync()
        {
            try
            {
                IsDataLoaded = false;

                // ĐÃ SỬA: Hứng gói dữ liệu DTO tổng hợp của ngày hôm nay từ Server
                var responseData = await _danhSachKhamService.GetTodayPatientsAsync();

                // Tạo đối tượng thực thể DanhSachKhamBenh theo đúng Model của bạn
                var danhSachModel = new DanhSachKhamBenh
                {
                    NgayKham = responseData?.NgayKham ?? DateTime.Today,
                    ChiTietDanhSach = new List<ChiTietDanhSachKham>()
                };

                if (responseData != null && responseData.ChiTietDanhSach != null)
                {
                    // Lặp qua mảng con ChiTietDanhSach của gói tổng hợp để đổ vào Model
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

                    // Đổ dữ liệu cấu hình quy định 1 và tổng doanh thu thực tế đầu ngày từ Server vào AppState
                    AppState.Instance.SoLuongToiDaHeThong = responseData.SoBenhNhanToiDaNgay;
                    AppState.Instance.TongDoanhThuTrongNgay = responseData.TongDoanhThuNgay;
                }

                // CẤT VÀO KHO DÙNG CHUNG để các màn hình dùng chung vùng nhớ trên RAM Client
                AppState.Instance.DanhSachKhamHienTai = danhSachModel;

                IsDataLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp dữ liệu đầu ngày: {ex.Message}");
                // Phòng hờ sập mạng hoặc không có kết nối: Khởi tạo danh sách rỗng để app không crash
                AppState.Instance.DanhSachKhamHienTai = new DanhSachKhamBenh { NgayKham = DateTime.Today, ChiTietDanhSach = new List<ChiTietDanhSachKham>() };
            }
        }

        /// <summary>
        /// Hàm điều hướng tập trung xử lý chuyển đổi giao diện dựa trên cơ chế ViewModel-First
        /// </summary>
        public void Navigate(string targetView)
        {
            if (string.IsNullOrEmpty(targetView)) return;

            switch (targetView)
            {
                case "Dashboard":
                    CurrentView = new DashboardViewModel();
                    break;

                case "PatientList":
                    // ĐÃ SỬA: Truyền chính nó (this) vào để PatientListViewModel có thể gọi lệnh lật trang
                    CurrentView = new PatientListViewModel(this);
                    break;

                case "PatientLookup":
                    CurrentView = new PatientLookupView();
                    break;

                case "Invoice":
                    CurrentView = new InvoiceView();
                    break;

                case "Report":
                    CurrentView = new ReportView();
                    break;

                case "Update":
                    CurrentView = new UpdateRegulations();
                    break;

                default:
                    CurrentView = new DashboardViewModel();
                    break;
            }
        }

        private void Logout(object parameter)
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

            if (activeMainWindow != null)
            {
                activeMainWindow.Close();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}