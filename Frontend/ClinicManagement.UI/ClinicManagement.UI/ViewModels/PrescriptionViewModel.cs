using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Models;   // Gọi trọn vẹn bộ thực thể nghiệp vụ bạn vừa gửi
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class PrescriptionViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainViewModel;

        // --- THỰC THỂ MODEL GỐC ---
        private readonly BenhNhan _benhNhanHienTai;
        private string _trieuChung;
        private string _chanDoan;
        private string _loaiBenhSelected;

        private ObservableCollection<string> _danhSachLoaiBenh;
        private ObservableCollection<MedicineRowViewModel> _toaThuocDangKe;

        public event PropertyChangedEventHandler PropertyChanged;

        // --- BINDING LÊN UI ---
        public string PatientName => _benhNhanHienTai?.HoTen ?? "Không rõ";
        public string NgayKhamText { get; set; }

        public string TrieuChung { get => _trieuChung; set { _trieuChung = value; OnPropertyChanged(); } }
        public string ChanDoan { get => _chanDoan; set { _chanDoan = value; OnPropertyChanged(); } }
        public string LoaiBenhDuDoan { get => _loaiBenhSelected; set { _loaiBenhSelected = value; OnPropertyChanged(); } }

        public ObservableCollection<string> DanhSachLoaiBenh { get => _danhSachLoaiBenh; set { _danhSachLoaiBenh = value; OnPropertyChanged(); } }
        public ObservableCollection<MedicineRowViewModel> ToaThuocDangKe { get => _toaThuocDangKe; set { _toaThuocDangKe = value; OnPropertyChanged(); } }

        // --- COMMANDS ---
        public ICommand ThemThuocCommand { get; }
        public ICommand HoanTatKhamCommand { get; }
        public ICommand HuyKhamCommand { get; }

        /// <summary>
        /// BIỂU ĐỒ TUẦN TỰ: layThongTinBenhNhan() nạp dữ liệu hành chính từ Model BenhNhan gốc
        /// </summary>
        public PrescriptionViewModel(MainWindowViewModel mainViewModel, BenhNhan selectedPatient)
        {
            _mainViewModel = mainViewModel;
            _benhNhanHienTai = selectedPatient;

            NgayKhamText = DateTime.Today.ToString("dd/MM/yyyy");

            // Giả lập nạp danh sách Loại bệnh (Quy định 2)
            DanhSachLoaiBenh = new ObservableCollection<string> { "Bệnh tai mũi họng", "Bệnh sốt siêu vi", "Đau dạ dày" };
            ToaThuocDangKe = new ObservableCollection<MedicineRowViewModel>();

            ThemThuocCommand = new RelayCommand(o => ExecuteThemThuoc());
            HoanTatKhamCommand = new RelayCommand(o => ExecuteHoanTatKham());
            HuyKhamCommand = new RelayCommand(o => ExecuteHuyKham());
        }

        private void ExecuteThemThuoc()
        {
            ToaThuocDangKe.Add(new MedicineRowViewModel());
        }

        /// <summary>
        /// BIỂU ĐỒ TUẦN TỰ: lapPhieuKham() - Khởi tạo PhieuKhamBenh, Duyệt loop [mỗi thuốc] qua hàm ThemChiTietToaThuoc
        /// </summary>
        private void ExecuteHoanTatKham()
        {
            if (string.IsNullOrWhiteSpace(TrieuChung) || string.IsNullOrWhiteSpace(ChanDoan) || string.IsNullOrWhiteSpace(LoaiBenhDuDoan))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Triệu chứng, Chẩn đoán và chọn Loại bệnh!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. KHỞI TẠO ĐỐI TƯỢNG PHIẾU KHÁM MỚI (Bám sát bước "tạo phiếu khám")
            PhieuKhamBenh phieuKhamMoi = new PhieuKhamBenh
            {
                MaPhieuKham = "PK" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper(),
                NgayKham = DateTime.Today,
                TrieuChung = TrieuChung,
                TenLoaiBenh = LoaiBenhDuDoan,
                BenhNhanKham = _benhNhanHienTai // Gán thực thể bệnh nhân gốc vào phiếu khám
            };

            // 2. VÒNG LẶP LOOP [MỖI THUỐC]: Lấy đơn giá thực thể Thuoc và nạp vào ChiTietToaThuoc
            foreach (var dongUi in ToaThuocDangKe)
            {
                if (string.IsNullOrWhiteSpace(dongUi.TenThuoc)) continue;

                // Giả lập bước tạo thực thể Thuoc và bốc "lấy đơn giá" từ danh mục hệ thống công ty (Quy định)
                var thuocHeThong = new Thuoc
                {
                    MaThuoc = "T" + Guid.NewGuid().ToString().Substring(0, 3).ToUpper(),
                    TenThuoc = dongUi.TenThuoc,
                    DonGia = 12000, // Giả lập đơn giá gốc 12.000đ/viên
                    MaDonVi = "DV01"
                };

                // Giả lập thực thể Cách dùng
                var cachDungHeThong = new CachDung
                {
                    MaCachDung = "CD01",
                    MoTaCachDung = dongUi.CachDung ?? "Uống sau khi ăn no"
                };

                // Gọi hàm nghiệp vụ nội bộ của Huyền để tự động tính toán Thành tiền, bẫy đơn giá
                phieuKhamMoi.ThemChiTietToaThuoc(thuocHeThong, dongUi.SoLuong, cachDungHeThong);
            }

            // 3. ĐỒNG BỘ TRẠNG THÁI RA MÀN HÌNH DANH SÁCH KHÁM NGÀY
            var dsKham = AppState.Instance.DanhSachKhamHienTai;
            if (dsKham != null)
            {
                var caKham = dsKham.ChiTietDanhSach.Find(p => p.BenhNhan.MaBenhNhan == _benhNhanHienTai.MaBenhNhan);
                if (caKham != null)
                {
                    caKham.TrangThai = "Đã khám";
                }
                AppState.Instance.NotifyDataChanged(); // Đẩy xung tín hiệu kích hoạt Dashboard vẽ lại vòng tròn ca khám
            }

            // Giao diện hiển thị thông báo thành công bám sát luồng của biểu đồ tuần tự
            MessageBox.Show($"[MÃ BM2] Lập phiếu khám thành công!\n" +
                            $"Mã phiếu: {phieuKhamMoi.MaPhieuKham}\n" +
                            $"Bệnh nhân: {phieuKhamMoi.BenhNhanKham.HoTen}\n" +
                            $"Tổng số thuốc đã kê trong vòng lặp: {phieuKhamMoi.ChiTietToaThuoc.Count} loại thuốc.",
                            "Lưu thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            ExecuteHuyKham();
        }

        private void ExecuteHuyKham()
        {
            _mainViewModel.CurrentView = new PatientListViewModel(_mainViewModel);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Lớp bổ trợ dùng để Binding dữ liệu nhập thô từ các TextBox lặp (MedicineRow.xaml)
    /// </summary>
    public class MedicineRowViewModel : INotifyPropertyChanged
    {
        private string _tenThuoc;
        private string _donViTinh = "Viên";
        private int _soLuong = 1;
        private string _cachDung;

        public event PropertyChangedEventHandler PropertyChanged;

        public string TenThuoc { get => _tenThuoc; set { _tenThuoc = value; OnPropertyChanged(); } }
        public string DonViTinh { get => _donViTinh; set { _donViTinh = value; OnPropertyChanged(); } }
        public int SoLuong { get => _soLuong; set { _soLuong = value; OnPropertyChanged(); } }
        public string CachDung { get => _cachDung; set { _cachDung = value; OnPropertyChanged(); } }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}