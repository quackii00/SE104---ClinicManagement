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
        private readonly MainWindowViewModel _mainViewModel;

        private string _hoTen;
        private bool _isNam = true;
        private bool _isNu;
        private string _ngaySinhText;
        private string _diaChi;

        public event PropertyChangedEventHandler PropertyChanged;

        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }
        public bool IsNam { get => _isNam; set { _isNam = value; OnPropertyChanged(); } }
        public bool IsNu { get => _isNu; set { _isNu = value; OnPropertyChanged(); } }
        public string NgaySinhText { get => _ngaySinhText; set { _ngaySinhText = value; OnPropertyChanged(); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }

        public ICommand TimBenhNhanCommand { get; }
        public ICommand ThemCommand { get; }
        public ICommand HuyCommand { get; }

        public RecievePatientViewModel(MainWindowViewModel mainViewModel)
        {
            _danhSachKhamService = new DanhSachKhamService();
            _mainViewModel = mainViewModel;

            TimBenhNhanCommand = new RelayCommand(async o => await ExecuteTimBenhNhanAsync());
            ThemCommand = new RelayCommand(async o => await ExecuteThemAsync());
            HuyCommand = new RelayCommand(o => ExecuteHuy());
        }

        /// <summary>
        /// NÚT TÌM KIẾM: ĐÃ ĐỔI LUẬT - Tra cứu hồ sơ cũ trực tiếp bằng Họ Tên Tiếp tân vừa gõ
        /// </summary>
        private async Task ExecuteTimBenhNhanAsync()
        {
            if (string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Vui lòng nhập Họ tên vào ô trống trước khi bấm Tìm kiếm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gọi hàm tìm kiếm ngầm của Service (truyền chuỗi rỗng cho Sdt, truyền HoTen vào)
            var existingPatient = await _danhSachKhamService.TimBenhNhanTheoSdtAsync(string.Empty, HoTen);
            if (existingPatient != null)
            {
                // Nếu tìm thấy bệnh nhân cũ trùng tên trong DB, tự động điền các thông tin còn lại
                IsNam = existingPatient.GioiTinh == "Nam";
                IsNu = existingPatient.GioiTinh == "Nữ";
                NgaySinhText = existingPatient.NamSinh.ToString();
                DiaChi = existingPatient.DiaChi;

                MessageBox.Show($"Hệ thống tìm thấy hồ sơ cũ của bệnh nhân: {existingPatient.HoTen} (Mã: {existingPatient.MaBenhNhan})", "Tìm kiếm thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Không tìm thấy hồ sơ cũ nào tên '{HoTen}'. Hệ thống sẽ tự tạo mới khi bấm nút Thêm.", "Thông báo tra cứu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// NÚT THÊM: LUỒNG ĐI CHUẨN XÁC THEO BIỂU ĐỒ - DÙNG HỌ TÊN ĐỂ THỰC HIỆN RẼ NHÁNH ALT
        /// </summary>
        private async Task ExecuteThemAsync()
        {
            // 1. Chuẩn hóa thông tin đầu vào
            string gioiTinhXuly = IsNu ? "Nữ" : "Nam";
            int namSinhXuly = DateTime.Now.Year - 15;
            if (DateTime.TryParse(NgaySinhText, out DateTime parsedDate)) namSinhXuly = parsedDate.Year;
            else int.TryParse(NgaySinhText, out namSinhXuly);

            var testPatient = new BenhNhan { HoTen = HoTen, GioiTinh = gioiTinhXuly, NamSinh = namSinhXuly, DiaChi = DiaChi };

            // Thông điệp kiemTraThongTinHopLe()
            if (!testPatient.KiemTraThongTinHopLe())
            {
                MessageBox.Show("Thông tin không hợp lệ! Vui lòng điền đầy đủ Họ tên và Năm sinh.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra quy định giới hạn 40 người trong ngày
            var dsKham = AppState.Instance.DanhSachKhamHienTai;
            if (dsKham != null && !dsKham.KiemTraGioiHan(AppState.Instance.SoLuongToiDaHeThong))
            {
                MessageBox.Show($"Không thể tiếp nhận! Hôm nay phòng mạch đã đạt giới hạn tối tối đa {AppState.Instance.SoLuongToiDaHeThong} bệnh nhân.", "Thông báo quy định", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChiTietKhamItemDto finalPatientRecord = null;

            // 2. Chạy thông điệp timBenhNhan() bằng Họ Tên hướng vào Service
            var checkPatientExist = await _danhSachKhamService.TimBenhNhanTheoSdtAsync(string.Empty, HoTen);

            // 3. Rẽ nhánh alt: Kiểm tra xem bệnh nhân đã tồn tại hay chưa
            if (checkPatientExist != null)
            {
                // Nhánh [Bệnh nhân đã tồn tại]: Lấy luôn hồ sơ cũ
                finalPatientRecord = checkPatientExist;
            }
            else
            {
                // Nhánh [Bệnh nhân chưa tồn tại]: Chạy thông điệp taoBenhNhan() xuống DB
                var request = new DangKyKhamRequest { HoTen = HoTen, GioiTinh = gioiTinhXuly, NamSinh = namSinhXuly, DiaChi = DiaChi, NgayKham = DateTime.Today };
                finalPatientRecord = await _danhSachKhamService.TiepNhanBenhNhanAsync(request);
            }

            // 4. Chạy thông điệp themBenhNhan() đổ vào danh sách khám hiện tại
            if (finalPatientRecord != null && dsKham != null)
            {
                dsKham.ChiTietDanhSach.Add(new ChiTietDanhSachKham
                {
                    STT = finalPatientRecord.STT,
                    TrangThai = finalPatientRecord.TrangThai ?? "Chờ khám",
                    BenhNhan = new BenhNhan
                    {
                        MaBenhNhan = finalPatientRecord.MaBenhNhan,
                        HoTen = finalPatientRecord.HoTen,
                        GioiTinh = finalPatientRecord.GioiTinh,
                        NamSinh = finalPatientRecord.NamSinh,
                        DiaChi = finalPatientRecord.DiaChi
                    }
                });

                MessageBox.Show($"Tiếp nhận thành công bệnh nhân: {finalPatientRecord.HoTen}!\nSố thứ tự khám hiện tại: {finalPatientRecord.STT}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                _mainViewModel.CurrentView = new ClinicManagement.UI.ViewModels.PatientListViewModel(_mainViewModel);
            }
        }

        private void ExecuteHuy()
        {
            _mainViewModel.CurrentView = new ClinicManagement.UI.ViewModels.PatientListViewModel(_mainViewModel);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}