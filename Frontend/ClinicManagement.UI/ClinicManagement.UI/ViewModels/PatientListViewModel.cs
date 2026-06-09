using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class PatientListViewModel : INotifyPropertyChanged
    {
        private readonly DanhSachKhamService _danhSachKhamService;
        private readonly MainWindowViewModel _mainViewModel;

        private string _counterText;
        private string _ngayKhamText;
        private ObservableCollection<ChiTietDanhSachKham> _uiPatientsList;
        private ChiTietDanhSachKham _selectedPatientItem;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CounterText { get => _counterText; set { _counterText = value; OnPropertyChanged(); } }
        public string NgayKhamText { get => _ngayKhamText; set { _ngayKhamText = value; OnPropertyChanged(); } }
        public ObservableCollection<ChiTietDanhSachKham> UiPatientsList { get => _uiPatientsList; set { _uiPatientsList = value; OnPropertyChanged(); } }

        /// <summary>
        /// BỘ PHÁT TÍN HIỆU CLICK DÒNG: Đã sửa lỗi nuốt quyền khi Re-login
        /// </summary>
        public ChiTietDanhSachKham SelectedPatientItem
        {
            get => _selectedPatientItem;
            set
            {
                _selectedPatientItem = value;
                OnPropertyChanged();

                if (_selectedPatientItem != null)
                {
                   
                    string currentRole = AppState.Instance.CurrentUserRole?.Trim();

                    if (!string.IsNullOrEmpty(currentRole) &&
                       (currentRole.Equals("Bác sĩ", StringComparison.OrdinalIgnoreCase) ||
                        currentRole.Equals("Bac si", StringComparison.OrdinalIgnoreCase) ||
                        currentRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase)))
                    {

                        ExecuteMoPhieuKham(_selectedPatientItem);
                    }
                    else
                    {
            
                        MessageBox.Show($"Tài khoản của bạn (Vai trò: '{currentRole ?? "Chưa xác định"}') không có quyền hạn này!\nChức năng lập phiếu khám bệnh (BM2) chỉ dành riêng cho Bác sĩ.",
                                        "Truy cập bị từ chối",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                    }

                    // Đưa dòng chọn về null để reset trạng thái click trơn tru cho lần sau
                    _selectedPatientItem = null;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand GoToFormCommand { get; }
        public ICommand XoaBenhNhanCommand { get; }

        public PatientListViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _danhSachKhamService = new DanhSachKhamService();

            if (AppState.Instance.DanhSachKhamHienTai == null)
            {
                AppState.Instance.DanhSachKhamHienTai = new DanhSachKhamBenh
                {
                    NgayKham = DateTime.Today,
                    ChiTietDanhSach = new System.Collections.Generic.List<ChiTietDanhSachKham>()
                };
            }

            GoToFormCommand = new RelayCommand(o => ExecuteGoToForm());
            XoaBenhNhanCommand = new RelayCommand(o => ExecuteXoaBenhNhan(o as ChiTietDanhSachKham));

            // Đóng dấu lắng nghe kho dữ liệu tập trung AppState
            AppState.Instance.PropertyChanged += OnAppStatePropertyChanged;

            RefreshUI();
        }

        private void OnAppStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.Instance.DanhSachKhamHienTai) ||
                e.PropertyName == nameof(AppState.Instance.SoLuongToiDaHeThong))
            {
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            var danhSachGoc = AppState.Instance.DanhSachKhamHienTai;
            int maxHeThong = AppState.Instance.SoLuongToiDaHeThong;

            if (danhSachGoc != null)
            {
                UiPatientsList = new ObservableCollection<ChiTietDanhSachKham>(danhSachGoc.ChiTietDanhSach);
                CounterText = $"{danhSachGoc.SoLuongHienTai}/{maxHeThong}";
                NgayKhamText = danhSachGoc.NgayKham.ToString("dd/MM/yyyy");
            }
        }

        /// <summary>
        /// LUỒNG BIỂU ĐỒ TUẦN TỰ: Lật trang SPA và truyền đối tượng Model BenhNhan sang Controller Phiếu Khám
        /// </summary>
        private void ExecuteMoPhieuKham(ChiTietDanhSachKham selectedItem)
        {
            if (_mainViewModel != null && selectedItem != null && selectedItem.BenhNhan != null)
            {
                // Tháo gỡ lắng nghe tránh rò rỉ RAM ngầm
                AppState.Instance.PropertyChanged -= OnAppStatePropertyChanged;

                // Chuyển góc nhìn lớn sang màn hình Kê đơn lập phiếu khám
                _mainViewModel.CurrentView = new PrescriptionViewModel(_mainViewModel, selectedItem.BenhNhan);
            }
        }

        private void ExecuteGoToForm()
        {
            if (_mainViewModel != null)
            {
                AppState.Instance.PropertyChanged -= OnAppStatePropertyChanged;
                _mainViewModel.CurrentView = new RecievePatientViewModel(_mainViewModel);
            }
        }

        private void ExecuteXoaBenhNhan(ChiTietDanhSachKham itemCanXoa)
        {
            if (itemCanXoa == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa bệnh nhân '{itemCanXoa.BenhNhan.HoTen}' ra khỏi danh sách khám hôm nay không?",
                                         "Xác nhận xóa ca khám", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var dsKhamGoc = AppState.Instance.DanhSachKhamHienTai;
                if (dsKhamGoc != null && dsKhamGoc.ChiTietDanhSach != null)
                {
                    dsKhamGoc.ChiTietDanhSach.Remove(itemCanXoa);

                    for (int i = 0; i < dsKhamGoc.ChiTietDanhSach.Count; i++)
                    {
                        dsKhamGoc.ChiTietDanhSach[i].STT = i + 1;
                    }

                    AppState.Instance.NotifyDataChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}