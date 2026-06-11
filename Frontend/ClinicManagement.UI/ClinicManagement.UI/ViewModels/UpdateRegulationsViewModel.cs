using System;
using System.Collections.Generic;
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
    public class UpdateRegulationsViewModel : INotifyPropertyChanged
    {
        private readonly QuyDinhService _quyDinhService;
        private readonly DanhMucService _danhMucService;

        public event PropertyChangedEventHandler PropertyChanged;

        // ==========================================
        // 1. Ô NHẬP LIỆU THAM SỐ SỐ (QĐ1 / QĐ4)
        // ==========================================
        private string _soBenhNhanToiDaText = string.Empty;
        private string _tienKhamText = string.Empty;

        public string SoBenhNhanToiDaText { get => _soBenhNhanToiDaText; set { _soBenhNhanToiDaText = value; OnPropertyChanged(); } }
        public string TienKhamText { get => _tienKhamText; set { _tienKhamText = value; OnPropertyChanged(); } }

        // ==========================================
        // 2. SỐ LƯỢNG THỐNG KÊ HIỂN THỊ (QĐ2)
        // ==========================================
        private int _soLoaiBenh;
        private int _soLoaiThuoc;
        private int _soDonVi;
        private int _soCachDung;

        public int SoLoaiBenh { get => _soLoaiBenh; set { _soLoaiBenh = value; OnPropertyChanged(); } }
        public int SoLoaiThuoc { get => _soLoaiThuoc; set { _soLoaiThuoc = value; OnPropertyChanged(); } }
        public int SoDonVi { get => _soDonVi; set { _soDonVi = value; OnPropertyChanged(); } }
        public int SoCachDung { get => _soCachDung; set { _soCachDung = value; OnPropertyChanged(); } }

        // Trạng thái xử lý ngầm (Chặn spam nút click)
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        // ==========================================
        // 3. Ô NHẬP LIỆU THÊM MỚI DANH MỤC
        // ==========================================
        private string _newDiseaseText = string.Empty;
        private string _newMedicineText = string.Empty;
        private string _newMedicinePriceText = string.Empty;
        private string _newUnitText = string.Empty;
        private string _newUsageMethodText = string.Empty;

        public string NewDiseaseText { get => _newDiseaseText; set { _newDiseaseText = value; OnPropertyChanged(); } }
        public string NewMedicineText { get => _newMedicineText; set { _newMedicineText = value; OnPropertyChanged(); } }
        public string NewMedicinePriceText { get => _newMedicinePriceText; set { _newMedicinePriceText = value; OnPropertyChanged(); } }
        public string NewUnitText { get => _newUnitText; set { _newUnitText = value; OnPropertyChanged(); } }
        public string NewUsageMethodText { get => _newUsageMethodText; set { _newUsageMethodText = value; OnPropertyChanged(); } }

        // Đơn vị tính được chọn từ ComboBox khi thêm thuốc mới
        private DonViDto? _selectedUnitForNewMedicine;
        public DonViDto? SelectedUnitForNewMedicine { get => _selectedUnitForNewMedicine; set { _selectedUnitForNewMedicine = value; OnPropertyChanged(); } }

        // ==========================================
        // 4. DANH SÁCH HIỂN THỊ TRÊN GIAO DIỆN
        // ==========================================
        public ObservableCollection<LoaiBenhDto> DiseaseTypes { get; } = new();
        public ObservableCollection<ThuocDto> MedicineTypes { get; } = new();
        public ObservableCollection<DonViDto> UnitTypes { get; } = new();
        public ObservableCollection<CachDungDto> UsageMethods { get; } = new();

        // ==========================================
        // 5. HỆ THỐNG COMMANDS NÚT BẤM
        // ==========================================
        public ICommand CapNhatCommand { get; }
        public ICommand AddDiseaseCommand { get; }
        public ICommand RemoveDiseaseCommand { get; }
        public ICommand AddMedicineCommand { get; }
        public ICommand RemoveMedicineCommand { get; }
        public ICommand AddUnitCommand { get; }
        public ICommand RemoveUnitCommand { get; }
        public ICommand AddUsageMethodCommand { get; }
        public ICommand RemoveUsageMethodCommand { get; }

        public UpdateRegulationsViewModel()
        {
            _quyDinhService = new QuyDinhService();
            _danhMucService = new DanhMucService();

            CapNhatCommand = new RelayCommand(async _ => await ExecuteCapNhatAsync());

            AddDiseaseCommand = new RelayCommand(async _ => await AddDiseaseAsync());
            RemoveDiseaseCommand = new RelayCommand(async p => await RemoveDiseaseAsync(p as LoaiBenhDto));

            AddMedicineCommand = new RelayCommand(async _ => await AddMedicineAsync());
            RemoveMedicineCommand = new RelayCommand(async p => await RemoveMedicineAsync(p as ThuocDto));

            AddUnitCommand = new RelayCommand(async _ => await AddUnitAsync());
            RemoveUnitCommand = new RelayCommand(async p => await RemoveUnitAsync(p as DonViDto));

            AddUsageMethodCommand = new RelayCommand(async _ => await AddUsageMethodAsync());
            RemoveUsageMethodCommand = new RelayCommand(async p => await RemoveUsageMethodAsync(p as CachDungDto));

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                // 1. Tải tham số hệ thống
                var ts = await _quyDinhService.GetThamSoAsync();
                if (ts != null)
                {
                    SoBenhNhanToiDaText = ts.SoBenhNhanToiDaNgay.ToString(CultureInfo.InvariantCulture);
                    TienKhamText = ts.TienKham.ToString(CultureInfo.InvariantCulture);
                    AppState.Instance.SoLuongToiDaHeThong = ts.SoBenhNhanToiDaNgay;
                }

                // 2. Tải danh mục Loại bệnh
                var loaiBenhList = await _danhMucService.GetLoaiBenhAsync();
                DiseaseTypes.Clear();
                foreach (var item in loaiBenhList) DiseaseTypes.Add(item);

                // 3. Tải danh mục Thuốc
                var thuocList = await _danhMucService.GetThuocAsync();
                MedicineTypes.Clear();
                foreach (var item in thuocList) MedicineTypes.Add(item);

                // 4. Tải danh mục Đơn vị tính
                var donViList = await _danhMucService.GetDonViAsync();
                UnitTypes.Clear();
                if (donViList != null) foreach (var item in donViList) UnitTypes.Add(item);

                // 5. Tải danh mục Cách dùng
                var cachDungList = await _danhMucService.GetCachDungAsync();
                UsageMethods.Clear();
                foreach (var item in cachDungList) UsageMethods.Add(item);

                UpdateCounts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateRegulations LoadAsync] Thất bại: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateCounts()
        {
            SoLoaiBenh = DiseaseTypes.Count;
            SoLoaiThuoc = MedicineTypes.Count;
            SoDonVi = UnitTypes.Count;
            SoCachDung = UsageMethods.Count;
        }

        // ==================== LOGIC XỬ LÝ LOẠI BỆNH ====================
        private async Task AddDiseaseAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDiseaseText)) return;
            IsBusy = true;

            var request = new UpsertLoaiBenhRequest { TenLoaiBenh = NewDiseaseText.Trim() };
            var result = await _quyDinhService.AddLoaiBenhAsync(request);
            if (result != null)
            {
                DiseaseTypes.Add(result);
                NewDiseaseText = string.Empty;
                UpdateCounts();
            }
            IsBusy = false;
        }

        private async Task RemoveDiseaseAsync(LoaiBenhDto? item)
        {
            if (item == null) return;
            IsBusy = true;

            var response = await _quyDinhService.DeleteLoaiBenhAsync(item.Id); // 🌟 FIX: Đổi từ item.Id thành MaLoaiBenh
            if (response != null && !response.Message.Contains("thất bại"))
            {
                DiseaseTypes.Remove(item);
                UpdateCounts();
            }
            IsBusy = false;
        }

        // ==================== LOGIC XỬ LÝ THUỐC & ĐƠN GIÁ ====================
        private async Task AddMedicineAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMedicineText)) return;

            if (SelectedUnitForNewMedicine == null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính cho thuốc mới.", "Thông báo nghiệp vụ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(NewMedicinePriceText?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal donGia) || donGia <= 0)
            {
                MessageBox.Show("Đơn giá thuốc phải là số dương lớn hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;

            var request = new UpsertThuocRequest
            {
                TenThuoc = NewMedicineText.Trim(),
                MaDonVi = SelectedUnitForNewMedicine.MaDonVi,
                DonGia = donGia
            };

            var result = await _quyDinhService.AddThuocAsync(request);
            if (result != null)
            {
                result.TenDonVi = SelectedUnitForNewMedicine.TenDonVi;
                MedicineTypes.Add(result);

                NewMedicineText = string.Empty;
                NewMedicinePriceText = string.Empty;
                SelectedUnitForNewMedicine = null;
                UpdateCounts();
            }
            IsBusy = false;
        }

        private async Task RemoveMedicineAsync(ThuocDto? item)
        {
            if (item == null) return;
            IsBusy = true;

            var response = await _quyDinhService.DeleteThuocAsync(item.Id); // 🌟 FIX: Đổi từ item.Id thành MaThuoc
            if (response != null && !response.Message.Contains("thất bại"))
            {
                MedicineTypes.Remove(item);
                UpdateCounts();
            }
            IsBusy = false;
        }

        // ==================== LOGIC XỬ LÝ ĐƠN VỊ TÍNH ====================
        private async Task AddUnitAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUnitText)) return;
            IsBusy = true;

            var request = new UpsertDonViRequest { TenDonVi = NewUnitText.Trim() };
            var result = await _quyDinhService.AddDonViAsync(request);
            if (result != null)
            {
                UnitTypes.Add(result);
                NewUnitText = string.Empty;
                UpdateCounts();
            }
            IsBusy = false;
        }

        private async Task RemoveUnitAsync(DonViDto? item)
        {
            if (item == null) return;
            IsBusy = true;

            var response = await _quyDinhService.DeleteDonViAsync(item.Id); // 🌟 FIX: Đổi từ item.Id thành MaDonVi
            if (response != null && !response.Message.Contains("thất bại"))
            {
                UnitTypes.Remove(item);
                UpdateCounts();
            }
            IsBusy = false;
        }

        // ==================== LOGIC XỬ LÝ CÁCH DÙNG ====================
        private async Task AddUsageMethodAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUsageMethodText)) return;
            IsBusy = true;

            var request = new UpsertCachDungRequest { MoTaCachDung = NewUsageMethodText.Trim() };
            var result = await _quyDinhService.AddCachDungAsync(request);
            if (result != null)
            {
                UsageMethods.Add(result);
                NewUsageMethodText = string.Empty;
                UpdateCounts();
            }
            IsBusy = false;
        }

        private async Task RemoveUsageMethodAsync(CachDungDto? item)
        {
            if (item == null) return;
            IsBusy = true;

            var response = await _quyDinhService.DeleteCachDungAsync(item.Id); // 🌟 FIX: Đổi từ item.Id thành MaCachDung
            if (response != null && !response.Message.Contains("thất bại"))
            {
                UsageMethods.Remove(item);
                UpdateCounts();
            }
            IsBusy = false;
        }

        // ==================== NÚT LƯU THAM SỐ CHÍNH & GIÁ THUỐC (PUT) ====================
        private async Task ExecuteCapNhatAsync()
        {
            if (!int.TryParse(SoBenhNhanToiDaText?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int soBenhNhan) || soBenhNhan <= 0)
            {
                MessageBox.Show("Số bệnh nhân tối đa phải là số nguyên dương.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TienKhamText?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal tienKham) || tienKham < 0)
            {
                MessageBox.Show("Tiền khám phải là số >= 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;

                // 🌟 1. Cập nhật các tham số hệ thống chung (QĐ1 / QĐ4)
                var requestParam = new UpdateThamSoRequest
                {
                    SoBenhNhanToiDaNgay = soBenhNhan,
                    TienKham = tienKham
                };
                var resultParam = await _quyDinhService.UpdateThamSoAsync(requestParam);

                if (resultParam != null)
                {
                    AppState.Instance.SoLuongToiDaHeThong = resultParam.SoBenhNhanToiDaNgay;
                }

                // 🌟 2. CHÌA KHÓA PHÁ ÁN: Duyệt danh sách thuốc để lưu lại toàn bộ đơn giá/tên thuốc vừa sửa đổi trực tiếp trên lưới (QĐ4)
                foreach (var thuoc in MedicineTypes)
                {
                    var requestThuoc = new UpsertThuocRequest
                    {
                        TenThuoc = thuoc.TenThuoc,
                        MaDonVi = thuoc.MaDonVi,
                        DonGia = thuoc.DonGia
                    };

                    // Gọi endpoint PUT: api/quydinh/thuoc/{id} ở Backend để lưu
                    await _quyDinhService.UpdateThuocAsync(thuoc.Id, requestThuoc);
                }

                MessageBox.Show("Cập nhật toàn bộ tham số quy định và đơn giá hệ thống thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cập nhật thất bại.\n{ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                // Nạp lại dữ liệu chuẩn từ DB lên giao diện sau khi lưu xong
                await LoadAsync();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}