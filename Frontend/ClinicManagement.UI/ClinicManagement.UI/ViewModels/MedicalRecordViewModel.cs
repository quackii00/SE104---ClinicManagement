using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClinicManagement.UI.ViewModels
{
    public class MedicalRecordViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly LichSuKhamDto _history;
        private readonly BenhNhan _benhNhan;
        private readonly TraCuuBenhNhanResultDto _patientInfo;
        private readonly DanhMucService _danhMucService;

        public event PropertyChangedEventHandler PropertyChanged;

        public BenhNhan BenhNhan => _benhNhan;
        public string NgayKham => _history.NgayKham.ToString("dd/MM/yyyy");
        public string ChanDoanLoaiBenh => _history.TenLoaiBenh ?? "Chưa có chẩn đoán";

        // Sử dụng MedicineRowViewModel để tận dụng logic hiển thị từ PrescriptionViewModel
        public ObservableCollection<MedicineRowViewModel> PrescriptionDetails { get; } = new ObservableCollection<MedicineRowViewModel>();

        public ICommand BackCommand { get; }

        public MedicalRecordViewModel(MainWindowViewModel mainWindowViewModel,
                                      LichSuKhamDto history,
                                      BenhNhan benhNhan,
                                      TraCuuBenhNhanResultDto patientInfo)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _history = history;
            _benhNhan = benhNhan;
            _patientInfo = patientInfo;
            _danhMucService = new DanhMucService();

            BackCommand = new RelayCommand(p => ExecuteBack());

            // Tải danh mục và nạp dữ liệu
            _ = LoadPrescriptionDataAsync();
        }

        private async Task LoadPrescriptionDataAsync()
        {
            // Lấy danh mục thuốc và cách dùng để hiển thị tên
            var cachedThuoc = await _danhMucService.GetThuocAsync() ?? new List<ThuocDto>();
            var cachedCachDung = await _danhMucService.GetCachDungAsync() ?? new List<CachDungDto>();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in _history.ToaThuoc)
                {
                    var row = new MedicineRowViewModel(cachedThuoc, cachedCachDung, true);
                    row.SelectedThuoc = cachedThuoc.FirstOrDefault(t => t.MaThuoc == item.MaThuoc);
                    row.SoLuong = item.SoLuong;
                    row.SelectedCachDung = cachedCachDung.FirstOrDefault(c => c.MaCachDung == item.MaCachDung);

                    PrescriptionDetails.Add(row);
                }
            });
        }

        private void ExecuteBack()
        {
            _mainWindowViewModel.CurrentView = new MedicalHistoryViewModel(_mainWindowViewModel, _patientInfo);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}