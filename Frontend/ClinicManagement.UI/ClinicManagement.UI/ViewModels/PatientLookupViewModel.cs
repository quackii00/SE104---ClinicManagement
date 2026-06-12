
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicManagement.UI.ViewModels
{
    public class PatientLookupViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TraCuuService _traCuuService;
        private readonly DanhMucService _danhMucService;

        private string _hoTenSearch;
        private string _namSinhSearch;
        private string _soDienThoaiSearch;
        private bool _isNam;
        private bool _isNu;
        private DateTime? _ngayKhamSearch;
        private bool _isSearching;

        private ObservableCollection<TraCuuBenhNhanResultDto> _patients = new ObservableCollection<TraCuuBenhNhanResultDto>();
        private ObservableCollection<LoaiBenhDto> _loaiBenhList = new ObservableCollection<LoaiBenhDto>();
        private LoaiBenhDto _selectedLoaiBenh;

        public event PropertyChangedEventHandler PropertyChanged;

        // --- BINDING CÁC THAM SỐ TÌM KIẾM ---
        public string HoTenSearch { get => _hoTenSearch; set { _hoTenSearch = value; OnPropertyChanged(); } }
        public string NamSinhSearch { get => _namSinhSearch; set { _namSinhSearch = value; OnPropertyChanged(); } }
        public string SoDienThoaiSearch { get => _soDienThoaiSearch; set { _soDienThoaiSearch = value; OnPropertyChanged(); } }

        // Danh mục loại bệnh để lọc (mục đầu "Tất cả" = không lọc).
        public ObservableCollection<LoaiBenhDto> LoaiBenhList { get => _loaiBenhList; set { _loaiBenhList = value; OnPropertyChanged(); } }
        public LoaiBenhDto SelectedLoaiBenh { get => _selectedLoaiBenh; set { _selectedLoaiBenh = value; OnPropertyChanged(); } }

        public bool IsNam
        {
            get => _isNam;
            set { _isNam = value; OnPropertyChanged(); if (value) IsNu = false; }
        }
        public bool IsNu
        {
            get => _isNu;
            set { _isNu = value; OnPropertyChanged(); if (value) IsNam = false; }
        }

        public DateTime? NgayKhamSearch { get => _ngayKhamSearch; set { _ngayKhamSearch = value; OnPropertyChanged(); } }
        public bool IsSearching { get => _isSearching; set { _isSearching = value; OnPropertyChanged(); } }


        public ObservableCollection<TraCuuBenhNhanResultDto> Patients { get => _patients; set { _patients = value; OnPropertyChanged(); } }

        private TraCuuBenhNhanResultDto _selectedPatientItem;
        public TraCuuBenhNhanResultDto SelectedPatientItem
        {
            get => _selectedPatientItem;
            set
            {
                if (value != null)
                {
                    _selectedPatientItem = value;
                    OnPropertyChanged();
                    ProcessPatientSelection(_selectedPatientItem);
                }
            }
        }

        public ICommand TimKiemCommand { get; }

        public PatientLookupViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _traCuuService = new TraCuuService();
            _danhMucService = new DanhMucService();

            TimKiemCommand = new RelayCommand(async o => await ExecuteTimKiemAsync());

            _isNam = false;
            _isNu = false;

            _ = LoadLoaiBenhAsync();
        }

        /// <summary>Nạp danh mục loại bệnh cho bộ lọc (kèm mục "Tất cả" = không lọc).</summary>
        private async Task LoadLoaiBenhAsync()
        {
            var list = await _danhMucService.GetLoaiBenhAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoaiBenhList.Clear();
                LoaiBenhList.Add(new LoaiBenhDto { Id = 0, TenLoaiBenh = "Tất cả" });
                foreach (var lb in list) LoaiBenhList.Add(lb);
                SelectedLoaiBenh = LoaiBenhList[0];
            });
        }

        /// <summary>
        /// Gửi lệnh tra cứu đa tham số lên Server (Chuẩn 4 tham số theo Controller Backend)
        /// </summary>
        private async Task ExecuteTimKiemAsync()
        {
            try
            {
                IsSearching = true;

                string hoTen = string.IsNullOrWhiteSpace(HoTenSearch) ? null : HoTenSearch.Trim();

                int? namSinh = null;
                if (!string.IsNullOrWhiteSpace(NamSinhSearch) && int.TryParse(NamSinhSearch, out int parsedNamSinh))
                {
                    namSinh = parsedNamSinh;
                }

                string gioiTinh = null;
                if (IsNam) gioiTinh = "Nam";
                else if (IsNu) gioiTinh = "Nữ";

                string soDienThoai = string.IsNullOrWhiteSpace(SoDienThoaiSearch) ? null : SoDienThoaiSearch.Trim();
                int? loaiBenhId = (SelectedLoaiBenh != null && SelectedLoaiBenh.Id > 0) ? SelectedLoaiBenh.Id : (int?)null;

                DateTime? ngayKhamYeuCau = null;
                if (NgayKhamSearch.HasValue)
                {
                    ngayKhamYeuCau = DateTime.SpecifyKind(NgayKhamSearch.Value.Date, DateTimeKind.Utc);
                }

                var resultList = await _traCuuService.TraCuuBenhNhanAsync(hoTen, namSinh, gioiTinh, ngayKhamYeuCau, soDienThoai, loaiBenhId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Patients.Clear();
                    if (resultList != null && resultList.Count > 0)
                    {
                        int index = 1;
                        foreach (var item in resultList)
                        {
                            Patients.Add(new TraCuuBenhNhanResultDto
                            {
                                STT = index++,
                                MaBenhNhan = item.MaBenhNhan,
                                HoTen = item.HoTen,
                                GioiTinh = item.GioiTinh,
                                NamSinh = item.NamSinh,
                                SoDienThoai = item.SoDienThoai,
                                NgayKham = item.NgayKham,
                                TenLoaiBenh = string.IsNullOrEmpty(item.TenLoaiBenh) ? "Chưa có" : item.TenLoaiBenh,
                                TrieuChung = string.IsNullOrEmpty(item.TrieuChung) ? "Không có" : item.TrieuChung
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy bệnh nhân nào khớp với bộ lọc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PatientLookupViewModel] Lỗi tra cứu: {ex.Message}");
                MessageBox.Show("Đã xảy ra sự cố kết nối trong quá trình tra cứu dữ liệu.", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// Kích hoạt khi click chọn một dòng bệnh nhân: Chuyển cảnh vèo sang trang Lịch sử khám bệnh
        /// </summary>
        private void ProcessPatientSelection(TraCuuBenhNhanResultDto selectedItem)
        {
            if (selectedItem == null) return;

            System.Diagnostics.Debug.WriteLine($"[PatientLookup] Click chọn bệnh nhân: {selectedItem.HoTen}, Mã: {selectedItem.MaBenhNhan}");

            _mainWindowViewModel.CurrentView = new MedicalHistoryViewModel(_mainWindowViewModel, selectedItem);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}