
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

        private string _maBenhNhan;
        private string _hoTen;
        private string _gioiTinh;
        private int _namSinh;
        private string _diaChi;
        private bool _isLoading;
        private ObservableCollection<LichSuKhamDto> _histories = new ObservableCollection<LichSuKhamDto>();

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ViewPhieuCommand { get; }

        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }
        public string GioiTinh { get => _gioiTinh; set { _gioiTinh = value; OnPropertyChanged(); } }
        public int NamSinh { get => _namSinh; set { _namSinh = value; OnPropertyChanged(); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        public ObservableCollection<LichSuKhamDto> Histories { get => _histories; set { _histories = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public MedicalHistoryViewModel(MainWindowViewModel mainWindowViewModel, TraCuuBenhNhanResultDto selectedPatient)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _traCuuService = new TraCuuService();

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
                        foreach (var history in historyList)
                        {
                            Histories.Add(history);
                        }

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
                MessageBox.Show("Không thể tải danh sách lịch sử bệnh án từ máy chủ.", "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteViewPhieu(LichSuKhamDto selectedHistory)
        {
            if (selectedHistory == null) return;

            System.Diagnostics.Debug.WriteLine($"[MedicalHistory] Đang mở giao diện PrescriptionView cho phiếu: {selectedHistory.MaPhieuKham}");

            _mainWindowViewModel.CurrentView = new PrescriptionViewModel(
                _mainWindowViewModel,
                this,
                HoTen,
                selectedHistory
            );
        }

        private void ExecuteBack()
        {
            _mainWindowViewModel.Navigate("PatientLookup");
        }

        private void ExecuteViewDetail(LichSuKhamDto selectedHistory)
        {
            if (selectedHistory == null) return;

            if (selectedHistory.ToaThuoc == null || selectedHistory.ToaThuoc.Count == 0)
            {
                MessageBox.Show("Ca khám này bác sĩ không tiến hành kê toa thuốc uống.", "Thông báo đơn thuốc", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string chiTietToa = "--- TOA THUỐC ĐI KÈM ---\n\n";
            int count = 1;
            foreach (var thuoc in selectedHistory.ToaThuoc)
            {
                chiTietToa += $"{count++}. Mã thuốc: {thuoc.MaThuoc} - Số lượng: {thuoc.SoLuong} viên\n" +
                      $"   Cách dùng: {thuoc.MaCachDung}\n\n";
            }

            MessageBox.Show(chiTietToa, "Chi tiết đơn thuốc", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string selectedPatient_GetDiaChiSafe(List<LichSuKhamDto> list)
        {
            return AppState.Instance.DanhSachKhamHienTai?.ChiTietDanhSach?
                .Find(p => p.BenhNhan?.MaBenhNhan == _maBenhNhan)?.BenhNhan?.DiaChi
                ?? "KTX Khu A - ĐHQG TP.HCM";
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}