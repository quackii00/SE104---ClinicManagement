using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class RecievePatientViewModel : INotifyPropertyChanged
    {
        private readonly DanhSachKhamService _danhSachKhamService;
        private readonly TraCuuService _traCuuService;
        private readonly MainWindowViewModel _mainViewModel;

        private Visibility _isAddButtonVisible = Visibility.Collapsed;
        private string _hoTen;
        private bool _isNam = true;
        private string _ngaySinhText;
        private string _diaChi;

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility IsAddButtonVisible
        {
            get => _isAddButtonVisible;
            set { _isAddButtonVisible = value; OnPropertyChanged(); }
        }

        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }
        public bool IsNam { get => _isNam; set { _isNam = value; OnPropertyChanged(); } }
        public bool IsNu { get => !_isNam; set { _isNam = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNam)); } }
        public string NgaySinhText { get => _ngaySinhText; set { _ngaySinhText = value; OnPropertyChanged(); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }

        public ICommand TimBenhNhanCommand { get; }
        public ICommand ThemCommand { get; }
        public ICommand HuyCommand { get; }

        public RecievePatientViewModel(MainWindowViewModel mainViewModel, DanhSachKhamService danhSachKhamService)
        {
            _mainViewModel = mainViewModel;
            _danhSachKhamService = danhSachKhamService;
            _traCuuService = new TraCuuService();

            TimBenhNhanCommand = new RelayCommand(async o => await ExecuteTimBenhNhanAsync());
            ThemCommand = new RelayCommand(async o => await ExecuteThemAsync());
            HuyCommand = new RelayCommand(async o => await ExecuteHuyAsync());

            CheckUserRolePermissions();
        }

        private void CheckUserRolePermissions()
        {
            string role = AppState.Instance.CurrentUserRole?.ToLower() ?? "";
            if (role.Contains("tiếp tân") || role.Contains("tieptan") || role.Contains("admin"))
            {
                IsAddButtonVisible = Visibility.Visible;
            }
            else
            {
                IsAddButtonVisible = Visibility.Collapsed;
            }
        }

        private async Task ExecuteTimBenhNhanAsync()
        {
            if (string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Vui lòng nhập Họ tên để tra cứu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? namSinh = null;
            if (!string.IsNullOrWhiteSpace(NgaySinhText) && int.TryParse(NgaySinhText, out int parsedNamSinh))
            {
                namSinh = parsedNamSinh;
            }

            string gioiTinh = IsNam ? "Nam" : "Nữ";
            DateTime ngayKhamChuan = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            var list = await _traCuuService.TraCuuBenhNhanAsync(HoTen.Trim(), namSinh, gioiTinh, ngayKhamChuan);

            if (list != null && list.Count > 0)
            {
                var patient = list[0];
                IsNam = patient.GioiTinh == "Nam";
                NgaySinhText = patient.NamSinh.ToString();
                DiaChi = patient.DiaChi;
                MessageBox.Show("Đã tìm thấy thông tin hồ sơ bệnh nhân cũ.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không tìm thấy hồ sơ cũ khớp với các thông tin trên.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task ExecuteThemAsync()
        {
            if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(NgaySinhText))
            {
                MessageBox.Show("Vui lòng điền đủ Họ tên và Năm sinh!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(NgaySinhText, out int namSinh))
            {
                MessageBox.Show("Năm sinh phải là số nguyên hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dsKham = AppState.Instance.DanhSachKhamHienTai;
            int maxPatients = AppState.Instance.SoLuongToiDaHeThong > 0 ? AppState.Instance.SoLuongToiDaHeThong : 40;
            if (dsKham != null && dsKham.ChiTietDanhSach != null && dsKham.ChiTietDanhSach.Count >= maxPatients)
            {
                MessageBox.Show($"Phòng mạch đã đủ số lượng quy định trong ngày ({maxPatients} người)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string gioiTinh = IsNam ? "Nam" : "Nữ";
            DateTime localToday = DateTime.Today;
            DateTime ngayKhamChuan = new DateTime(localToday.Year, localToday.Month, localToday.Day, 0, 0, 0, DateTimeKind.Utc);

            var list = await _traCuuService.TraCuuBenhNhanAsync(HoTen.Trim(), namSinh, gioiTinh, ngayKhamChuan);
            ChiTietKhamItemDto record = null;

            if (list != null && list.Count > 0)
            {
                var target = list[0];
                record = new ChiTietKhamItemDto
                {
                    MaBenhNhan = target.MaBenhNhan,
                    HoTen = target.HoTen,
                    GioiTinh = target.GioiTinh,
                    NamSinh = target.NamSinh,
                    DiaChi = target.DiaChi
                };
            }
            else
            {
                var request = new DangKyKhamRequest
                {
                    HoTen = HoTen.Trim(),
                    GioiTinh = gioiTinh,
                    NamSinh = namSinh,
                    DiaChi = DiaChi,
                    NgayKham = ngayKhamChuan
                };

                System.Diagnostics.Debug.WriteLine($"====================================");
                System.Diagnostics.Debug.WriteLine($"[DEBUG GIỜ C#] localToday: {localToday}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG GIỜ C#] ngayKhamChuan: {ngayKhamChuan.ToString("o")}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG GIỜ C#] request.NgayKham: {request.NgayKham:yyyy-MM-dd HH:mm:ss} Kind: {request.NgayKham.Kind}");
                System.Diagnostics.Debug.WriteLine($"====================================");

                record = await _danhSachKhamService.TiepNhanBenhNhanAsync(request);
            }

            if (record == null) return;

            if (dsKham != null)
            {
                if (dsKham.ChiTietDanhSach == null)
                {
                    dsKham.ChiTietDanhSach = new System.Collections.Generic.List<ChiTietDanhSachKham>();
                }

                var newItem = new ChiTietDanhSachKham
                {
                    STT = record.STT > 0 ? record.STT : (dsKham.ChiTietDanhSach.Count + 1),
                    TrangThai = string.IsNullOrEmpty(record.TrangThai) ? "Chờ khám" : record.TrangThai,
                    BenhNhan = new BenhNhan
                    {
                        MaBenhNhan = record.MaBenhNhan,
                        HoTen = record.HoTen,
                        GioiTinh = record.GioiTinh,
                        NamSinh = record.NamSinh,
                        DiaChi = record.DiaChi
                    }
                };

                dsKham.ChiTietDanhSach.Add(newItem);

                AppState.Instance.TriggerDashboardUpdate();

                MessageBox.Show($"Tiếp nhận thành công bệnh nhân: {record.HoTen}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                var targetPatientListVM = new PatientListViewModel(_mainViewModel);
                _mainViewModel.CurrentView = targetPatientListVM;
                await targetPatientListVM.LoadTodayPatientsDataAsync();
            }
        }

        private async Task ExecuteHuyAsync()
        {
            _mainViewModel.CurrentView = new PatientListViewModel(_mainViewModel);
            await Task.CompletedTask;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}