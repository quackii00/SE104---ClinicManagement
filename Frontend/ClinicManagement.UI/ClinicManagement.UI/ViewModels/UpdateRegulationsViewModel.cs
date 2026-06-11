using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    /// <summary>
    /// YC6 – ViewModel cho màn hình "Cập nhật quy định" (Views/UI/Update/UpdateRegulations.xaml).
    /// - Khi mở: GET api/quydinh để đổ giá trị hiện hành lên form.
    /// - Khi bấm "Cập nhật": PUT api/quydinh để lưu QĐ1 (số BN tối đa/ngày) + QĐ4 (tiền khám) vào CSDL.
    /// </summary>
    public class UpdateRegulationsViewModel : INotifyPropertyChanged
    {
        private readonly QuyDinhService _quyDinhService;

        public event PropertyChangedEventHandler PropertyChanged;

        // ----- QĐ1 & QĐ4: các ô NHẬP LIỆU (gửi lên Server qua PUT) -----
        private string _soBenhNhanToiDaText = string.Empty;
        private string _tienKhamText = string.Empty;

        public string SoBenhNhanToiDaText
        {
            get => _soBenhNhanToiDaText;
            set { _soBenhNhanToiDaText = value; OnPropertyChanged(); }
        }

        public string TienKhamText
        {
            get => _tienKhamText;
            set { _tienKhamText = value; OnPropertyChanged(); }
        }

        // ----- QĐ2: các SỐ ĐẾM danh mục (chỉ đọc, do Server tính sẵn) -----
        private int _soLoaiBenh;
        private int _soLoaiThuoc;
        private int _soCachDung;

        public int SoLoaiBenh { get => _soLoaiBenh; set { _soLoaiBenh = value; OnPropertyChanged(); } }
        public int SoLoaiThuoc { get => _soLoaiThuoc; set { _soLoaiThuoc = value; OnPropertyChanged(); } }
        public int SoCachDung { get => _soCachDung; set { _soCachDung = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        // ----- Danh sách lựa chọn danh mục (quản lý cục bộ trên form) -----
        // TODO (mở rộng): nạp ItemsSource từ GET api/danhmuc/* và POST item mới qua api/quydinh/{loaibenh|thuoc|cachdung}.
        public ObservableCollection<string> DiseaseTypes { get; } = new();
        public ObservableCollection<string> SelectedDiseaseTypes { get; } = new();
        private string _selectedDiseaseType;
        public string SelectedDiseaseType { get => _selectedDiseaseType; set { _selectedDiseaseType = value; OnPropertyChanged(); } }

        public ObservableCollection<string> MedicineTypes { get; } = new();
        public ObservableCollection<string> SelectedMedicineTypes { get; } = new();
        private string _selectedMedicineType;
        public string SelectedMedicineType { get => _selectedMedicineType; set { _selectedMedicineType = value; OnPropertyChanged(); } }

        public ObservableCollection<string> UsageMethods { get; } = new();
        public ObservableCollection<string> SelectedUsageMethods { get; } = new();
        private string _selectedUsageMethod;
        public string SelectedUsageMethod { get => _selectedUsageMethod; set { _selectedUsageMethod = value; OnPropertyChanged(); } }

        // ----- Commands -----
        public ICommand CapNhatCommand { get; }
        public ICommand AddDiseaseCommand { get; }
        public ICommand RemoveDiseaseCommand { get; }
        public ICommand AddMedicineCommand { get; }
        public ICommand RemoveMedicineCommand { get; }
        public ICommand AddUsageMethodCommand { get; }
        public ICommand RemoveUsageMethodCommand { get; }

        public UpdateRegulationsViewModel()
        {
            _quyDinhService = new QuyDinhService();

            CapNhatCommand = new RelayCommand(async _ => await ExecuteCapNhatAsync());

            AddDiseaseCommand = new RelayCommand(_ => AddSelection(SelectedDiseaseType, SelectedDiseaseTypes));
            RemoveDiseaseCommand = new RelayCommand(p => SelectedDiseaseTypes.Remove(p?.ToString()));
            AddMedicineCommand = new RelayCommand(_ => AddSelection(SelectedMedicineType, SelectedMedicineTypes));
            RemoveMedicineCommand = new RelayCommand(p => SelectedMedicineTypes.Remove(p?.ToString()));
            AddUsageMethodCommand = new RelayCommand(_ => AddSelection(SelectedUsageMethod, SelectedUsageMethods));
            RemoveUsageMethodCommand = new RelayCommand(p => SelectedUsageMethods.Remove(p?.ToString()));

            // Nạp quy định hiện hành ngay khi mở màn hình (không chặn UI)
            _ = LoadAsync();
        }

        /// <summary>GET api/quydinh → đổ giá trị đang áp dụng lên form.</summary>
        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                var ts = await _quyDinhService.GetThamSoAsync();
                if (ts != null)
                {
                    SoBenhNhanToiDaText = ts.SoBenhNhanToiDaNgay.ToString(CultureInfo.InvariantCulture);
                    TienKhamText = ts.TienKham.ToString(CultureInfo.InvariantCulture);
                    SoLoaiBenh = ts.SoLoaiBenh;
                    SoLoaiThuoc = ts.SoLoaiThuoc;
                    SoCachDung = ts.SoCachDung;

                    // Đồng bộ luôn giới hạn xuống kho dùng chung để các màn hình khác dùng đúng số mới
                    AppState.Instance.SoLuongToiDaHeThong = ts.SoBenhNhanToiDaNgay;
                }
            }
            catch (Exception ex)
            {
                // Mất kết nối / chưa đăng nhập: dùng tạm giá trị đang có trong AppState để form không trống
                System.Diagnostics.Debug.WriteLine($"[UpdateRegulations LoadAsync]: {ex.Message}");
                SoBenhNhanToiDaText = AppState.Instance.SoLuongToiDaHeThong.ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// NÚT "CẬP NHẬT": kiểm tra hợp lệ → PUT api/quydinh → cập nhật lại AppState + thông báo.
        /// </summary>
        private async Task ExecuteCapNhatAsync()
        {
            // 1) Kiểm tra hợp lệ phía Client (Backend cũng kiểm tra lại lần nữa)
            if (!int.TryParse(SoBenhNhanToiDaText?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int soBenhNhan) || soBenhNhan <= 0)
            {
                MessageBox.Show("Số bệnh nhân tối đa/ngày phải là số nguyên dương.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TienKhamText?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal tienKham) || tienKham < 0)
            {
                MessageBox.Show("Tiền khám phải là số ≥ 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;

                // 2) Gửi gói cập nhật LÊN Server bằng PUT
                var request = new UpdateThamSoRequest
                {
                    SoBenhNhanToiDaNgay = soBenhNhan,
                    TienKham = tienKham
                };
                var result = await _quyDinhService.UpdateThamSoAsync(request);

                // 3) QuyDinhService trả null nếu Server báo lỗi / mất kết nối
                if (result == null)
                {
                    MessageBox.Show("Cập nhật thất bại. Kiểm tra kết nối hoặc quyền Admin rồi thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Server đã lưu vào CSDL & trả về số liệu mới nhất → cập nhật lại UI + kho dùng chung
                SoLoaiBenh = result.SoLoaiBenh;
                SoLoaiThuoc = result.SoLoaiThuoc;
                SoCachDung = result.SoCachDung;
                AppState.Instance.SoLuongToiDaHeThong = result.SoBenhNhanToiDaNgay;

                MessageBox.Show("Cập nhật quy định thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cập nhật thất bại.\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static void AddSelection(string value, ObservableCollection<string> target)
        {
            if (!string.IsNullOrWhiteSpace(value) && !target.Contains(value))
                target.Add(value);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
