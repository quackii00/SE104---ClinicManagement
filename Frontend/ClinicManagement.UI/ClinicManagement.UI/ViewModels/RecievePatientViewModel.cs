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

        // --- CÁC THUỘC TÍNH BINDING RA GIAO DIỆN XAML ---
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

        // --- HÀM KHỞI TẠO (CONSTRUCTOR) ---
        public RecievePatientViewModel(MainWindowViewModel mainViewModel, DanhSachKhamService danhSachKhamService)
        {
            _mainViewModel = mainViewModel;
            _danhSachKhamService = danhSachKhamService;
            _traCuuService = new TraCuuService();

            TimBenhNhanCommand = new RelayCommand(async o => await ExecuteTimBenhNhanAsync());
            ThemCommand = new RelayCommand(async o => await ExecuteThemAsync());
            HuyCommand = new RelayCommand(async o => await ExecuteHuyAsync());

            // Kiểm tra phân quyền ẩn/hiện nút thêm của tài khoản đang đăng nhập
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

        /// <summary>
        /// Logic nút bấm Tra Cứu bệnh nhân cũ bằng TraCuuService chuyên trách
        /// </summary>
        private async Task ExecuteTimBenhNhanAsync()
        {
            if (string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Vui lòng nhập Họ tên để tra cứu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var list = await _traCuuService.TraCuuBenhNhanAsync(HoTen, null, null, null);

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
                MessageBox.Show("Không tìm thấy hồ sơ cũ với tên này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Logic nút bấm Thêm/Tiếp nhận bệnh nhân vào danh sách khám ngày hôm nay
        /// </summary>
        private async Task ExecuteThemAsync()
        {
            // 1. Validate cơ bản dữ liệu đầu vào
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

            // 2. Kiểm tra giới hạn phòng mạch (Quy định tối đa số lượng ca khám)
            var dsKham = AppState.Instance.DanhSachKhamHienTai;
            int maxPatients = AppState.Instance.SoLuongToiDaHeThong > 0 ? AppState.Instance.SoLuongToiDaHeThong : 40;
            if (dsKham != null && dsKham.ChiTietDanhSach != null && dsKham.ChiTietDanhSach.Count >= maxPatients)
            {
                MessageBox.Show($"Phòng mạch đã đủ số lượng quy định trong ngày ({maxPatients} người)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Rẽ nhánh xử lý: Dùng chung TraCuuService quét kiểm tra trùng lặp hồ sơ trước khi thêm
            var list = await _traCuuService.TraCuuBenhNhanAsync(HoTen, null, null, null);
            ChiTietKhamItemDto record = null;

            if (list != null && list.Count > 0)
            {
                var target = list[0];
                // Người cũ: Ép kiểu trực tiếp từ lịch sử tra cứu sang DTO kết quả tiếp nhận
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
                // Người mới: Gửi gói tin POST lên API
                // 🌟 SỬA ĐỔI QUYẾT ĐỊNH: Chỉ định cụ thể dạng ngày UTC để gỡ bỏ Giờ địa phương thừa gây lỗi 400 Bad Request trên Server
                DateTime ngayKhamChuan = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

                var request = new DangKyKhamRequest
                {
                    HoTen = HoTen,
                    GioiTinh = IsNam ? "Nam" : "Nữ",
                    NamSinh = namSinh,
                    DiaChi = DiaChi,
                    NgayKham = ngayKhamChuan
                };

                record = await _danhSachKhamService.TiepNhanBenhNhanAsync(request);
            }

            // 🌟 CHỐT CHẶN AN TOÀN: Nếu dính lỗi phân quyền (403) hoặc lỗi JSON (400) làm record bị null, đứng im giữ UI để đổi tài khoản
            if (record == null) return;

            // 4. Cập nhật dữ liệu thật nhận về từ Server vào bộ nhớ đệm AppState để ListView hiển thị đồng bộ
            if (dsKham != null)
            {
                if (dsKham.ChiTietDanhSach == null)
                {
                    dsKham.ChiTietDanhSach = new System.Collections.Generic.List<ChiTietDanhSachKham>();
                }

                dsKham.ChiTietDanhSach.Add(new ChiTietDanhSachKham
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
                });

                // Kích hoạt đồng bộ số liệu biểu đồ cho Dashboard màn hình chính
                AppState.Instance.TriggerDashboardUpdate();

                MessageBox.Show($"Tiếp nhận thành công bệnh nhân: {record.HoTen}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Điều hướng quay trở về màn hình danh sách khám bệnh hôm nay
                _mainViewModel.CurrentView = new PatientListViewModel(_mainViewModel);
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