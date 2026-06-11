
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

        private string _hoTenSearch;
        private string _namSinhSearch;
        private bool _isNam;
        private bool _isNu;
        private DateTime? _ngayKhamSearch;
        private bool _isSearching;

        private ObservableCollection<TraCuuBenhNhanResultDto> _patients = new ObservableCollection<TraCuuBenhNhanResultDto>();

        public event PropertyChangedEventHandler PropertyChanged;

        // --- BINDING CÁC THAM SỐ TÌM KIẾM ---
        public string HoTenSearch { get => _hoTenSearch; set { _hoTenSearch = value; OnPropertyChanged(); } }
        public string NamSinhSearch { get => _namSinhSearch; set { _namSinhSearch = value; OnPropertyChanged(); } }

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

            TimKiemCommand = new RelayCommand(async o => await ExecuteTimKiemAsync());

            _isNam = false;
            _isNu = false;
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

                DateTime? ngayKhamYeuCau = null;
                if (NgayKhamSearch.HasValue)
                {
                    ngayKhamYeuCau = DateTime.SpecifyKind(NgayKhamSearch.Value.Date, DateTimeKind.Utc);
                }

                var resultList = await _traCuuService.TraCuuBenhNhanAsync(hoTen, namSinh, gioiTinh, ngayKhamYeuCau);

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