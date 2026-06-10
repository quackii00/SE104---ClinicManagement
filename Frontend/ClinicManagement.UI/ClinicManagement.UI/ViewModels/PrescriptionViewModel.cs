using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class PrescriptionViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly BenhNhan _benhNhanHienTai;
        private readonly PhieuKhamService _phieuKhamService;
        private readonly DanhMucService _danhMucService;
        private readonly string _maPhieuKhamHienTai;

        public event PropertyChangedEventHandler PropertyChanged;

        // --- BINDING PROPERTIES ---
        public string PatientName => _benhNhanHienTai?.HoTen ?? "Không rõ";
        public string NgayKhamText { get; set; } = DateTime.Today.ToString("dd/MM/yyyy");

        private string _trieuChung;
        private string _benhDuocChanDoan;
        private bool _isReadOnly;
        private Visibility _addButtonVisibility = Visibility.Visible;

        public string TrieuChung { get => _trieuChung; set { _trieuChung = value; OnPropertyChanged(); } }
        public string BenhDuocChanDoan { get => _benhDuocChanDoan; set { _benhDuocChanDoan = value; OnPropertyChanged(); } }

        public bool IsReadOnly { get => _isReadOnly; set { _isReadOnly = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsEditable)); } }
        public bool IsEditable => !_isReadOnly;

        public Visibility AddButtonVisibility { get => _addButtonVisibility; set { _addButtonVisibility = value; OnPropertyChanged(); } }

        public ObservableCollection<LoaiBenhDto> DanhSachLoaiBenh { get; set; } = new ObservableCollection<LoaiBenhDto>();
        public List<ThuocDto> CachedThuocList { get; private set; } = new List<ThuocDto>();
        public List<CachDungDto> CachedCachDungList { get; private set; } = new List<CachDungDto>();

        public ObservableCollection<MedicineRowViewModel> ToaThuocDangKe { get; } = new ObservableCollection<MedicineRowViewModel>();

        // --- COMMANDS ---
        public ICommand ThemThuocCommand { get; }
        public ICommand HoanTatKhamCommand { get; }
        public ICommand HuyKhamCommand { get; }

        public PrescriptionViewModel(MainWindowViewModel mainViewModel, BenhNhan selectedPatient, string maPhieuKhamCu = "")
        {
            _mainViewModel = mainViewModel;
            _benhNhanHienTai = selectedPatient;
            _maPhieuKhamHienTai = maPhieuKhamCu;
            _phieuKhamService = new PhieuKhamService();
            _danhMucService = new DanhMucService();

            ThemThuocCommand = new RelayCommand(o =>
            {
                if (IsReadOnly) return;
                ToaThuocDangKe.Add(CreateMedicineRow());
            });

            HoanTatKhamCommand = new RelayCommand(async o => {
                if (IsReadOnly) return;
                await ExecuteHoanTatKhamAsync();
            });

            HuyKhamCommand = new RelayCommand(async o =>
            {
                _mainViewModel.CurrentView = new PatientListViewModel(_mainViewModel);
                await Task.CompletedTask;
            });

            // Mặc định ban đầu luôn mở khóa để các luồng nạp data không bị chặn gán thuộc tính
            IsReadOnly = false;
            AddButtonVisibility = Visibility.Visible;

            _ = InitializeDataAsync();
        }

        /// <summary>
        /// Luồng tổng hợp bốc thông tin danh mục và hồ sơ khám cũ
        /// </summary>
        private async Task InitializeDataAsync()
        {
            try
            {
                var loaiBenhList = await _danhMucService.GetLoaiBenhAsync() ?? new List<LoaiBenhDto>();
                CachedThuocList = await _danhMucService.GetThuocAsync() ?? new List<ThuocDto>();
                CachedCachDungList = await _danhMucService.GetCachDungAsync() ?? new List<CachDungDto>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DanhSachLoaiBenh.Clear();
                    foreach (var item in loaiBenhList)
                    {
                        DanhSachLoaiBenh.Add(item);
                    }
                    OnPropertyChanged(nameof(DanhSachLoaiBenh));
                });

                // 🌟 XỬ LÝ CA ĐÃ KHÁM: Bốc dữ liệu cũ
                if (!string.IsNullOrEmpty(_maPhieuKhamHienTai))
                {
                    var phieuCu = await _phieuKhamService.GetPhieuKhamByIdAsync(_maPhieuKhamHienTai);

                    if (phieuCu != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 1. Đổ dữ liệu chữ thuần túy trước
                            TrieuChung = phieuCu.TrieuChung;
                            BenhDuocChanDoan = phieuCu.MaLoaiBenh;

                            ToaThuocDangKe.Clear();

                            if (phieuCu.ToaThuoc != null && phieuCu.ToaThuoc.Count > 0)
                            {
                                foreach (var itemThuoc in phieuCu.ToaThuoc)
                                {
                                    // 🌟 BÍ QUYẾT: Khởi tạo dòng thuốc ở trạng thái MỞ KHÓA (false) để gán dữ liệu trơn tru
                                    var row = new MedicineRowViewModel(CachedThuocList, CachedCachDungList, false);

                                    row.XoaThuocCommand = new RelayCommand(o => {
                                        if (IsReadOnly) return;
                                        ToaThuocDangKe.Remove(row);
                                    });

                                    // So khớp đối tượng từ mảng Cache để ComboBox tìm đúng vị trí dòng dữ liệu
                                    row.SelectedThuoc = CachedThuocList.FirstOrDefault(t => t.MaThuoc == itemThuoc.MaThuoc);
                                    row.SoLuong = itemThuoc.SoLuong;
                                    row.SelectedCachDung = CachedCachDungList.FirstOrDefault(c => c.MaCachDung == itemThuoc.MaCachDung);

                                    // 🌟 KHÓA RIÊNG: Sau khi gán data xong xuôi mới kích hoạt khóa cứng dòng thuốc này lại
                                    row.IsRowEnabled = false;

                                    ToaThuocDangKe.Add(row);
                                }
                            }

                            // 2. CHỐT CHẶN CUỐI: Sau khi dữ liệu đã map đầy đủ lên UI mới chính thức khóa cứng toàn Form mẹ
                            IsReadOnly = true;
                            AddButtonVisibility = Visibility.Collapsed;
                        });
                        return;
                    }
                }

                // Nếu là ca mới tinh chưa khám, tự động tạo sẵn một dòng thuốc trống
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ToaThuocDangKe.Count == 0)
                    {
                        ToaThuocDangKe.Add(CreateMedicineRow());
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PrescriptionViewModel] Lỗi InitializeDataAsync: {ex.Message}");
            }
        }

        private MedicineRowViewModel CreateMedicineRow()
        {
            var row = new MedicineRowViewModel(CachedThuocList, CachedCachDungList, IsReadOnly);
            row.XoaThuocCommand = new RelayCommand(o =>
            {
                if (IsReadOnly) return;
                ToaThuocDangKe.Remove(row);
            });
            return row;
        }

        private async Task ExecuteHoanTatKhamAsync()
        {
            if (string.IsNullOrWhiteSpace(TrieuChung) || string.IsNullOrWhiteSpace(BenhDuocChanDoan))
            {
                MessageBox.Show("Vui lòng điền đầy đủ Triệu chứng và Chẩn đoán loại bệnh!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime ngayKhamChuanUtc = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            var validToaThuocRequests = ToaThuocDangKe
                .Where(r => r.SelectedThuoc != null && !string.IsNullOrEmpty(r.SelectedThuoc.MaThuoc))
                .Select(r => new ChiTietToaThuocRequest
                {
                    MaThuoc = r.SelectedThuoc.MaThuoc,
                    SoLuong = r.SoLuong,
                    MaCachDung = r.SelectedCachDung?.MaCachDung ?? "CD01"
                }).ToList();

            var request = new CreatePhieuKhamRequest
            {
                MaBenhNhan = _benhNhanHienTai.MaBenhNhan,
                NgayKham = ngayKhamChuanUtc,
                TrieuChung = TrieuChung,
                MaLoaiBenh = BenhDuocChanDoan,
                ToaThuoc = validToaThuocRequests
            };

            var result = await _phieuKhamService.CreatePhieuKhamAsync(request);

            if (result != null)
            {
                MessageBox.Show("Lập phiếu khám bệnh thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                var patientInList = AppState.Instance.DanhSachKhamHienTai?.ChiTietDanhSach
                    .FirstOrDefault(p => p.BenhNhan.MaBenhNhan == _benhNhanHienTai.MaBenhNhan);
                if (patientInList != null)
                {
                    patientInList.TrangThai = "Đã khám";
                    patientInList.MaPhieuKham = result.MaPhieuKham;
                    AppState.Instance.TriggerDashboardUpdate();
                }

                _mainViewModel.CurrentView = new PatientListViewModel(_mainViewModel);
            }
            else
            {
                MessageBox.Show("Lập phiếu khám thất bại! Hãy kiểm tra cửa sổ Output Debug để xem phản hồi chi tiết từ Server.", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MedicineRowViewModel : INotifyPropertyChanged
    {
        private ThuocDto _selectedThuoc;
        private CachDungDto _selectedCachDung;
        private int _soLuong = 1;
        private bool _isRowEnabled = true; // Chuyển sang biến thường để dễ gán ép trạng thái công khai

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand XoaThuocCommand { get; set; }

        public ThuocDto SelectedThuoc
        {
            get => _selectedThuoc;
            set { _selectedThuoc = value; OnPropertyChanged(); OnPropertyChanged(nameof(DonViTinh)); }
        }

        public CachDungDto SelectedCachDung
        {
            get => _selectedCachDung;
            set { _selectedCachDung = value; OnPropertyChanged(); }
        }

        public string DonViTinh => SelectedThuoc?.TenDonVi ?? "";
        public int SoLuong { get => _soLuong; set { _soLuong = value; OnPropertyChanged(); } }

        // 🌟 PROPERTY ĐỒNG BỘ: Cho phép thay đổi linh hoạt trạng thái đóng mở từ bên ngoài lớp
        public bool IsRowEnabled
        {
            get => _isRowEnabled;
            set { _isRowEnabled = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ThuocDto> DanhSachThuocDto { get; set; }
        public ObservableCollection<CachDungDto> DanhSachCachDungDto { get; set; }

        public MedicineRowViewModel(List<ThuocDto> thuocSource, List<CachDungDto> cachDungSource, bool isLocked = false)
        {
            _isRowEnabled = !isLocked; // Nếu bị Lock từ đầu thì IsEnabled = false, ngược lại = true
            DanhSachThuocDto = new ObservableCollection<ThuocDto>(thuocSource);
            DanhSachCachDungDto = new ObservableCollection<CachDungDto>(cachDungSource);

            _selectedCachDung = DanhSachCachDungDto.FirstOrDefault();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}