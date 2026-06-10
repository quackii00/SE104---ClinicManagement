using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Models;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class InvoiceViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly HoaDonService _hoaDonService;

        private decimal _examinationFee;
        private decimal _totalMedicineCost;
        private decimal _totalAmount;
        private bool _isProcessing;
        private DateTime _invoiceDate = DateTime.Today;
        private ObservableCollection<ChiTietToaThuocDto> _invoiceMedicines = new ObservableCollection<ChiTietToaThuocDto>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ChiTietDanhSachKham CurrentPatientContext { get; }

        public decimal ExaminationFee
        {
            get => _examinationFee;
            set { _examinationFee = value; OnPropertyChanged(); }
        }

        public decimal TotalMedicineCost
        {
            get => _totalMedicineCost;
            set { _totalMedicineCost = value; OnPropertyChanged(); }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set { _isProcessing = value; OnPropertyChanged(); }
        }

        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set { _invoiceDate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChiTietToaThuocDto> InvoiceMedicines
        {
            get => _invoiceMedicines;
            set { _invoiceMedicines = value; OnPropertyChanged(); }
        }

        public ICommand PaymentCommand { get; }

        public InvoiceViewModel(MainWindowViewModel mainViewModel, ChiTietDanhSachKham patientContext)
        {
            _mainViewModel = mainViewModel;
            CurrentPatientContext = patientContext;
            _hoaDonService = new HoaDonService();

            PaymentCommand = new RelayCommand(async o => await ExecutePaymentAsync());

            _ = InitializeInvoiceDataAsync();
        }

        private async Task InitializeInvoiceDataAsync()
        {
            try
            {
                if (CurrentPatientContext == null || string.IsNullOrEmpty(CurrentPatientContext.MaPhieuKham))
                {
                    MessageBox.Show("Bệnh nhân chưa có thông tin phiếu khám để lập hóa đơn!", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var invoiceData = await _hoaDonService.PreviewHoaDonAsync(CurrentPatientContext.MaPhieuKham);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (invoiceData != null)
                    {
                        ExaminationFee = invoiceData.TienKham;
                        TotalMedicineCost = invoiceData.TienThuoc;
                        TotalAmount = invoiceData.TongTien;
                        InvoiceDate = invoiceData.NgayKham;

                        InvoiceMedicines.Clear();
                        if (invoiceData.ChiTietThuoc != null)
                        {
                            foreach (var item in invoiceData.ChiTietThuoc)
                            {
                                InvoiceMedicines.Add(item);
                            }
                        }
                    }
                    else
                    {
                        ExaminationFee = 30000;
                        TotalMedicineCost = 0;
                        TotalAmount = 30000;
                        InvoiceDate = DateTime.Today;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InvoiceViewModel] Lỗi nạp hóa đơn: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ExaminationFee = 30000;
                    TotalAmount = 30000;
                    InvoiceDate = DateTime.Today;
                });
            }
        }

        private async Task ExecutePaymentAsync()
        {
            if (IsProcessing) return;

            try
            {
                IsProcessing = true;

                if (CurrentPatientContext == null || string.IsNullOrEmpty(CurrentPatientContext.MaPhieuKham))
                {
                    MessageBox.Show("Không tìm thấy mã phiếu khám hợp lệ để thanh toán.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = await _hoaDonService.CreateHoaDonAsync(CurrentPatientContext.MaPhieuKham);

                if (result != null)
                {
                    MessageBox.Show("Thanh toán và lưu hóa đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (AppState.Instance.DanhSachKhamHienTai?.ChiTietDanhSach != null)
                    {
                        var target = AppState.Instance.DanhSachKhamHienTai.ChiTietDanhSach
                            .Find(p => p.BenhNhan.MaBenhNhan == CurrentPatientContext.BenhNhan.MaBenhNhan);

                        if (target != null)
                        {
                            target.TrangThai = "Đã thanh toán";
                        }
                    }

                    AppState.Instance.TriggerDashboardUpdate();

                    _mainViewModel.CurrentView = new PatientListViewModel(_mainViewModel);
                }
                else
                {
                    MessageBox.Show("Xử lý giao dịch thanh toán thất bại từ hệ thống Server.", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InvoiceViewModel] Lỗi thanh toán: {ex.Message}");
                MessageBox.Show("Đã xảy ra sự cố trong quá trình xử lý thanh toán hóa đơn.", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}