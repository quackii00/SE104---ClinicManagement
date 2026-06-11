using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    /// <summary>
    /// YC5 / BM5.1 – ViewModel cho tab "Báo cáo tháng" (doanh thu).
    /// Bấm "Lập báo cáo" → GET api/baocao/doanhthu → đổ DataGrid + tổng doanh thu.
    /// </summary>
    public class MonthlyReportViewModel : INotifyPropertyChanged
    {
        private readonly BaoCaoService _baoCaoService;

        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime? _selectedDate = DateTime.Today;
        public DateTime? SelectedDate { get => _selectedDate; set { _selectedDate = value; OnPropertyChanged(); } }

        public ObservableCollection<DoanhThuItemDto> ChiTiet { get; } = new();

        private decimal _tongDoanhThu;
        public decimal TongDoanhThu { get => _tongDoanhThu; set { _tongDoanhThu = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand LapBaoCaoCommand { get; }

        public MonthlyReportViewModel()
        {
            _baoCaoService = new BaoCaoService();
            LapBaoCaoCommand = new RelayCommand(async _ => await ExecuteLapBaoCaoAsync());
        }

        private async Task ExecuteLapBaoCaoAsync()
        {
            if (SelectedDate is null)
            {
                MessageBox.Show("Vui lòng chọn tháng cần lập báo cáo.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                var data = await _baoCaoService.GetDoanhThuAsync(SelectedDate.Value.Month, SelectedDate.Value.Year);

                ChiTiet.Clear();
                if (data?.ChiTiet != null)
                {
                    foreach (var item in data.ChiTiet)
                        ChiTiet.Add(item);
                }
                TongDoanhThu = data?.TongDoanhThu ?? 0m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lập báo cáo thất bại.\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
