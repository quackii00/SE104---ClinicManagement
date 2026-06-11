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
    public class ReportViewModel : INotifyPropertyChanged
    {
        private readonly BaoCaoService _baoCaoService;
        private int _selectedMonthNumber = DateTime.Today.Month;
        private int _selectedYearNumber = DateTime.Today.Year;
        private decimal _totalMonthlyRevenue;
        private bool _isDataLoading;

        private ObservableCollection<DoanhThuItemDto> _monthlyReportItems = new ObservableCollection<DoanhThuItemDto>();
        private ObservableCollection<SuDungThuocItemDto> _medicineReportItems = new ObservableCollection<SuDungThuocItemDto>();

        public event PropertyChangedEventHandler PropertyChanged;

        public int SelectedMonthNumber
        {
            get => _selectedMonthNumber;
            set
            {
                if (_selectedMonthNumber != value)
                {
                    _selectedMonthNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedYearNumber
        {
            get => _selectedYearNumber;
            set
            {
                if (_selectedYearNumber != value)
                {
                    _selectedYearNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalMonthlyRevenue
        {
            get => _totalMonthlyRevenue;
            set { _totalMonthlyRevenue = value; OnPropertyChanged(); }
        }

        public bool IsDataLoading
        {
            get => _isDataLoading;
            set { _isDataLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DoanhThuItemDto> MonthlyReportItems
        {
            get => _monthlyReportItems;
            set { _monthlyReportItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SuDungThuocItemDto> MedicineReportItems
        {
            get => _medicineReportItems;
            set { _medicineReportItems = value; OnPropertyChanged(); }
        }

        public ICommand LoadMonthlyReportCommand { get; }
        public ICommand LoadReportCommand { get; }

        public ReportViewModel()
        {
            _baoCaoService = new BaoCaoService();

            LoadMonthlyReportCommand = new RelayCommand(async o => await ExecuteLoadMonthlyReportAsync(), o => !IsDataLoading);
            LoadReportCommand = new RelayCommand(async o => await ExecuteLoadMedicineReportAsync(), o => !IsDataLoading);
        }

        private async Task ExecuteLoadMonthlyReportAsync()
        {
            if (IsDataLoading) return;
            try
            {
                IsDataLoading = true;
                var data = await _baoCaoService.GetMonthlyRevenueReportAsync(SelectedMonthNumber, SelectedYearNumber);

                if (data == null)
                {
                    MessageBox.Show("Backend trả về dữ liệu null. Kiểm tra Token đăng nhập hoặc phân quyền tài khoản (Phải là Kế toán/Admin).");
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MonthlyReportItems.Clear();
                    decimal sumRevenue = 0;

                    if (data.ChiTiet != null)
                    {
                        int stt = 1;
                        foreach (var item in data.ChiTiet)
                        {
                            item.STT = stt++;
                            MonthlyReportItems.Add(item);
                            sumRevenue += item.DoanhThu;
                        }
                    }
                    TotalMonthlyRevenue = sumRevenue;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối đến Backend: {ex.Message}\n{ex.InnerException?.Message}");
            }
            finally
            {
                IsDataLoading = false;
            }
        }

        private async Task ExecuteLoadMedicineReportAsync()
        {
            if (IsDataLoading) return;
            try
            {
                IsDataLoading = true;
                var data = await _baoCaoService.GetMedicineUsageReportAsync(SelectedMonthNumber, SelectedYearNumber);

                if (data == null)
                {
                    MessageBox.Show("Backend trả về dữ liệu null. Kiểm tra Token đăng nhập hoặc phân quyền tài khoản (Phải là Kế toán/Admin).");
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MedicineReportItems.Clear();
                    if (data.ChiTiet != null)
                    {
                        int stt = 1;
                        foreach (var item in data.ChiTiet)
                        {
                            item.STT = stt++;
                            MedicineReportItems.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối đến Backend: {ex.Message}\n{ex.InnerException?.Message}");
            }
            finally
            {
                IsDataLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}