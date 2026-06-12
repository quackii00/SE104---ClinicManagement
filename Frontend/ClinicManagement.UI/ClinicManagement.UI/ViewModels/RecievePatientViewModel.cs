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
        private string _soDienThoai;

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
        public string SoDienThoai { get => _soDienThoai; set { _soDienThoai = value; OnPropertyChanged(); } }

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
            // BM Tiếp nhận: tra cứu hồ sơ cũ THEO SỐ ĐIỆN THOẠI để tự điền thông tin.
            if (string.IsNullOrWhiteSpace(SoDienThoai))
            {
                MessageBox.Show("Vui lòng nhập Số điện thoại để tra cứu hồ sơ cũ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var bn = await _traCuuService.TimBenhNhanTheoSdtAsync(SoDienThoai.Trim());

            if (bn != null)
            {
                HoTen = bn.HoTen;
                IsNam = bn.GioiTinh == "Nam";
                NgaySinhText = bn.NamSinh.ToString();
                DiaChi = bn.DiaChi;
                MessageBox.Show("Đã tìm thấy hồ sơ bệnh nhân cũ theo số điện thoại.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không tìm thấy hồ sơ cũ với số điện thoại này. Vui lòng nhập thông tin mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

            // QĐ/YC1: năm sinh phải từ 1900 đến năm hiện tại (không nhỏ hơn 1900, không ở tương lai).
            int namHienTai = DateTime.Now.Year;
            if (namSinh < 1900 || namSinh > namHienTai)
            {
                MessageBox.Show($"Năm sinh phải từ 1900 đến {namHienTai}!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            // Luôn gọi backend tiếp nhận: server tự DÙNG LẠI hồ sơ cũ nếu trùng SĐT, ngược lại tạo mới.
            var request = new DangKyKhamRequest
            {
                HoTen = HoTen.Trim(),
                GioiTinh = gioiTinh,
                NamSinh = namSinh,
                DiaChi = DiaChi,
                SoDienThoai = string.IsNullOrWhiteSpace(SoDienThoai) ? null : SoDienThoai.Trim(),
                NgayKham = ngayKhamChuan
            };

            var record = await _danhSachKhamService.TiepNhanBenhNhanAsync(request);

            if (record == null)
            {
                // Hiện đúng lý do từ server (vd: đã đủ 40 BN/ngày, bệnh nhân đã có trong danh sách...).
                string msg = !string.IsNullOrWhiteSpace(_danhSachKhamService.LastErrorMessage)
                    ? _danhSachKhamService.LastErrorMessage
                    : "Không thể tiếp nhận bệnh nhân. Vui lòng thử lại.";
                MessageBox.Show(msg, "Tiếp nhận thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Tiếp nhận thành công bệnh nhân: {record.HoTen}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Tải lại danh sách từ server để có STT/SĐT/trạng thái chuẩn (tránh tự dựng item lệch dữ liệu).
            var targetPatientListVM = new PatientListViewModel(_mainViewModel);
            _mainViewModel.CurrentView = targetPatientListVM;
            await targetPatientListVM.LoadTodayPatientsDataAsync();
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