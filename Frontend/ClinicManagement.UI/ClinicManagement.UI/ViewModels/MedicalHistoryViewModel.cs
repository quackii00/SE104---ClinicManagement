using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicManagement.UI.ViewModels
{
    public class MedicalHistoryViewModel : INotifyPropertyChanged
    {
        private readonly TraCuuService _traCuuService;
        private readonly MainWindowViewModel _mainWindowViewModel;

        // Đảm bảo namespace ClinicManagement.UI.DTOs đã được using ở trên
        private readonly TraCuuBenhNhanResultDto _selectedPatient;

        private string _maBenhNhan;
        private string _hoTen;
        private string _gioiTinh;
        private int _namSinh;
        private string _diaChi;
        private bool _isLoading;
        private ObservableCollection<LichSuKhamDto> _histories = new ObservableCollection<LichSuKhamDto>();

        public event PropertyChangedEventHandler PropertyChanged;

        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }
        public string GioiTinh { get => _gioiTinh; set { _gioiTinh = value; OnPropertyChanged(); } }
        public int NamSinh { get => _namSinh; set { _namSinh = value; OnPropertyChanged(); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        public ObservableCollection<LichSuKhamDto> Histories { get => _histories; set { _histories = value; OnPropertyChanged(); } }

        public ICommand ViewPhieuCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public MedicalHistoryViewModel(MainWindowViewModel mainWindowViewModel, TraCuuBenhNhanResultDto selectedPatient)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _traCuuService = new TraCuuService();
            _selectedPatient = selectedPatient;

            _maBenhNhan = selectedPatient.MaBenhNhan;
            HoTen = selectedPatient.HoTen;
            GioiTinh = selectedPatient.GioiTinh;
            NamSinh = selectedPatient.NamSinh;

            DiaChi = "Đang tải dữ liệu địa chỉ...";

            ViewPhieuCommand = new RelayCommand(p => ExecuteViewPhieu(p as LichSuKhamDto));
            BackCommand = new RelayCommand(p => ExecuteBack());
            ViewDetailCommand = new RelayCommand(p => ExecuteViewDetail(p as LichSuKhamDto));

            _ = LoadLichSuKhamFromServerAsync();
        }

        private async Task LoadLichSuKhamFromServerAsync()
        {
            try
            {
                IsLoading = true;
                if (string.IsNullOrEmpty(_maBenhNhan)) return;

                var historyList = await _traCuuService.GetLichSuKhamAsync(_maBenhNhan);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Histories.Clear();
                    if (historyList != null && historyList.Count > 0)
                    {
                        foreach (var history in historyList) Histories.Add(history);
                        DiaChi = selectedPatient_GetDiaChiSafe(historyList);
                    }
                    else
                    {
                        DiaChi = "Chưa cập nhật địa chỉ thường trú.";
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MedicalHistoryViewModel] Lỗi tải lịch sử: {ex.Message}");
                MessageBox.Show("Không thể tải danh sách lịch sử bệnh án.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteViewPhieu(LichSuKhamDto selectedHistory)
        {
            if (selectedHistory == null) return;
            var patient = new BenhNhan
            {
                HoTen = this.HoTen,
                GioiTinh = this.GioiTinh,
                NamSinh = this.NamSinh
            };
            _mainWindowViewModel.CurrentView = new MedicalRecordViewModel(_mainWindowViewModel, selectedHistory, patient, _selectedPatient);
        }

        private void ExecuteBack() => _mainWindowViewModel.Navigate("PatientLookup");

        private void ExecuteViewDetail(LichSuKhamDto selectedHistory)
        {
            if (selectedHistory == null) return;

            var patient = new BenhNhan
            {
                HoTen = this.HoTen,
                GioiTinh = this.GioiTinh,
                // Chuyển int sang string bằng .ToString()
                NamSinh = this.NamSinh
            };

            _mainWindowViewModel.CurrentView = new MedicalRecordViewModel(_mainWindowViewModel, selectedHistory, patient, _selectedPatient);
        }

        private string selectedPatient_GetDiaChiSafe(List<LichSuKhamDto> list)
        {
            // Lấy địa chỉ thật từ danh sách khám trong ngày nếu có; KHÔNG dùng địa chỉ giả.
            var diaChi = AppState.Instance.DanhSachKhamHienTai?.ChiTietDanhSach?
                .Find(p => p.BenhNhan?.MaBenhNhan == _maBenhNhan)?.BenhNhan?.DiaChi;
            return string.IsNullOrWhiteSpace(diaChi) ? "Chưa cập nhật địa chỉ thường trú." : diaChi;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}