using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class PatientListViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly DanhSachKhamService _danhSachKhamService;
        private readonly MainWindowViewModel _mainViewModel;

        private string _counterText;
        private string _ngayKhamText;
        private ObservableCollection<ChiTietDanhSachKham> _uiPatientsList;
        private ChiTietDanhSachKham _selectedPatientItem;
        private bool _isDataLoading;
        private Visibility _isAddButtonVisible = Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CounterText { get => _counterText; set { _counterText = value; OnPropertyChanged(); } }
        public string NgayKhamText { get => _ngayKhamText; set { _ngayKhamText = value; OnPropertyChanged(); } }
        public ObservableCollection<ChiTietDanhSachKham> UiPatientsList { get => _uiPatientsList; set { _uiPatientsList = value; OnPropertyChanged(); } }
        public bool IsDataLoading { get => _isDataLoading; set { _isDataLoading = value; OnPropertyChanged(); } }
        public Visibility IsAddButtonVisible { get => _isAddButtonVisible; set { _isAddButtonVisible = value; OnPropertyChanged(); } }

        public ChiTietDanhSachKham SelectedPatientItem
        {
            get => _selectedPatientItem;
            set
            {
                if (value != null)
                {
                    _selectedPatientItem = value;
                    OnPropertyChanged();
                    ProcessPatientSelection(_selectedPatientItem);
                    _selectedPatientItem = null;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand GoToFormCommand { get; }
        public ICommand XoaBenhNhanCommand { get; }
        public ICommand RefreshCommand { get; }

        public PatientListViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _danhSachKhamService = new DanhSachKhamService();

            GoToFormCommand = new RelayCommand(o => ExecuteGoToForm());
            XoaBenhNhanCommand = new RelayCommand(o => ExecuteXoaBenhNhan(o as ChiTietDanhSachKham));
            RefreshCommand = new RelayCommand(async o => await LoadTodayPatientsDataAsync());

            AppState.Instance.PropertyChanged += OnAppStatePropertyChanged;

            RefreshUI();

            _ = LoadTodayPatientsDataAsync();
        }

        public async Task LoadTodayPatientsDataAsync()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => IsDataLoading = true);
                var responseData = await _danhSachKhamService.GetTodayPatientsAsync();

                var danhSachModel = new DanhSachKhamBenh
                {
                    Id = responseData?.Id ?? 0,
                    NgayKham = responseData?.NgayKham ?? DateTime.Today,
                    SoBenhNhanToiDaNgay = responseData?.SoBenhNhanToiDaNgay ?? 40,
                    TongDoanhThuNgay = responseData?.TongDoanhThuNgay ?? 0,
                    ChiTietDanhSach = new System.Collections.Generic.List<ChiTietDanhSachKham>()
                };

                var tempUiList = new ObservableCollection<ChiTietDanhSachKham>();

                if (responseData != null && responseData.ChiTietDanhSach != null)
                {
                    foreach (var item in responseData.ChiTietDanhSach)
                    {
                        var patientItem = new ChiTietDanhSachKham
                        {
                            STT = item.STT,
                            TrangThai = string.IsNullOrEmpty(item.TrangThai) ? "Chờ khám" : item.TrangThai,
                            MaPhieuKham = item.MaPhieuKham,
                            BenhNhan = new BenhNhan
                            {
                                MaBenhNhan = item.MaBenhNhan,
                                HoTen = item.HoTen,
                                GioiTinh = item.GioiTinh,
                                NamSinh = item.NamSinh,
                                DiaChi = item.DiaChi,
                                SoDienThoai = item.SoDienThoai
                            }
                        };
                        danhSachModel.ChiTietDanhSach.Add(patientItem);
                        tempUiList.Add(patientItem);
                    }

                    AppState.Instance.SoLuongToiDaHeThong = responseData.SoBenhNhanToiDaNgay;
                    AppState.Instance.TongDoanhThuTrongNgay = responseData.TongDoanhThuNgay;
                }

                AppState.Instance.PropertyChanged -= OnAppStatePropertyChanged;
                AppState.Instance.DanhSachKhamHienTai = danhSachModel;
                AppState.Instance.PropertyChanged += OnAppStatePropertyChanged;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UiPatientsList = tempUiList;
                    CounterText = $"{UiPatientsList.Count}/{AppState.Instance.SoLuongToiDaHeThong}";
                    CultureInfo cultureVi = new CultureInfo("vi-VN");
                    NgayKhamText = danhSachModel.NgayKham.ToString("dd/MM/yyyy", cultureVi);

                    string role = AppState.Instance.CurrentUserRole?.ToLower() ?? "";
                    if (role.Contains("tiếp tân") || role.Contains("tieptan"))
                    {
                        IsAddButtonVisible = Visibility.Visible;
                    }
                    else
                    {
                        IsAddButtonVisible = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientListViewModel] Lỗi tải danh sách: {ex.Message}");
                if (AppState.Instance.DanhSachKhamHienTai == null)
                {
                    AppState.Instance.DanhSachKhamHienTai = new DanhSachKhamBenh { NgayKham = DateTime.Today };
                }
                RefreshUI();
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => IsDataLoading = false);
            }
        }

        private void ProcessPatientSelection(ChiTietDanhSachKham item)
        {
            string role = AppState.Instance.CurrentUserRole?.ToLower() ?? "";

            // 🌟 1. BÁC SĨ CLICK -> VÀO TRANG KHÁM BỆNH
            if (role.Contains("bác sĩ") || role.Contains("doctor") || role.Contains("bacsi"))
            {
                string maPhieuKhamCũ = !string.IsNullOrEmpty(item.MaPhieuKham) ? item.MaPhieuKham : string.Empty;

                // Bệnh nhân chưa lập phiếu & đang "Chờ khám" -> chuyển sang "Đang khám" (3 trạng thái theo BM).
                if (string.IsNullOrEmpty(maPhieuKhamCũ) && item.TrangThai == "Chờ khám")
                {
                    item.TrangThai = "Đang khám";
                    _ = _danhSachKhamService.BatDauKhamAsync(item.BenhNhan?.MaBenhNhan);
                }

                Dispose();

                // Lưu ý: Nếu code báo đỏ ở dòng này, bạn hãy mở file PrescriptionViewModel.cs ra
                // và kiểm tra xem hàm khởi tạo của nó đang yêu cầu truyền vào mấy tham số nhé.
                _mainViewModel.CurrentView = new PrescriptionViewModel(_mainViewModel, item.BenhNhan, maPhieuKhamCũ);
            }
            // 🌟 2. TIẾP TÂN HOẶC KẾ TOÁN CLICK -> VÀO TRANG HÓA ĐƠN
            else if (role.Contains("tiếp tân") || role.Contains("tieptan") || role.Contains("kế toán") || role.Contains("ketoan"))
            {
                if (!string.IsNullOrEmpty(item.MaPhieuKham) && item.TrangThai != "Chờ khám")
                {
                    Dispose();

                    // Lưu ý: Tương tự ở đây, kiểm tra constructor của InvoiceViewModel nếu có lỗi đỏ.
                    _mainViewModel.CurrentView = new InvoiceViewModel(_mainViewModel, item);
                }
                else
                {
                    MessageBox.Show($"Bệnh nhân '{item.BenhNhan.HoTen}' đang ở trạng thái chờ, bác sĩ chưa kết thúc ca khám để lập hóa đơn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void OnAppStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.Instance.DanhSachKhamHienTai))
            {
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            var ds = AppState.Instance.DanhSachKhamHienTai;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ds != null && ds.ChiTietDanhSach != null)
                {
                    UiPatientsList = new ObservableCollection<ChiTietDanhSachKham>(ds.ChiTietDanhSach);
                    CounterText = $"{ds.ChiTietDanhSach.Count}/{AppState.Instance.SoLuongToiDaHeThong}";

                    CultureInfo cultureVi = new CultureInfo("vi-VN");
                    NgayKhamText = ds.NgayKham.ToString("dd/MM/yyyy", cultureVi);
                }
                else
                {
                    UiPatientsList = new ObservableCollection<ChiTietDanhSachKham>();
                    CounterText = $"0/{AppState.Instance.SoLuongToiDaHeThong}";
                    NgayKhamText = DateTime.Today.ToString("dd/MM/yyyy", new CultureInfo("vi-VN"));
                }

                string role = AppState.Instance.CurrentUserRole?.ToLower() ?? "";
                if (role.Contains("tiếp tân") || role.Contains("tieptan"))
                {
                    IsAddButtonVisible = Visibility.Visible;
                }
                else
                {
                    IsAddButtonVisible = Visibility.Collapsed;
                }
            });
        }

        private void ExecuteGoToForm()
        {
            string role = AppState.Instance.CurrentUserRole?.ToLower() ?? "";
            if (role.Contains("tiếp tân") || role.Contains("tieptan"))
            {
                Dispose();
                _mainViewModel.CurrentView = new RecievePatientViewModel(_mainViewModel, _danhSachKhamService);
            }
        }

        private void ExecuteXoaBenhNhan(ChiTietDanhSachKham item)
        {
            if (item == null) return;

            string role = AppState.Instance.CurrentUserRole?.ToLower() ?? "";
            if (!role.Contains("tiếp tân") && !role.Contains("tieptan")) return;

            if (MessageBox.Show($"Xóa bệnh nhân '{item.BenhNhan.HoTen}' khỏi danh sách hôm nay?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (AppState.Instance.DanhSachKhamHienTai?.ChiTietDanhSach != null)
                {
                    AppState.Instance.DanhSachKhamHienTai.ChiTietDanhSach.Remove(item);

                    for (int i = 0; i < AppState.Instance.DanhSachKhamHienTai.ChiTietDanhSach.Count; i++)
                    {
                        AppState.Instance.DanhSachKhamHienTai.ChiTietDanhSach[i].STT = i + 1;
                    }

                    AppState.Instance.TriggerDashboardUpdate();
                    RefreshUI();
                }
            }
        }

        public void Dispose()
        {
            AppState.Instance.PropertyChanged -= OnAppStatePropertyChanged;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}