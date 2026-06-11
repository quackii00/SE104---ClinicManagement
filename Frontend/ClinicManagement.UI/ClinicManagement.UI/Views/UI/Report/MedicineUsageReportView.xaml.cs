using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClinicManagement.UI.ViewModels;

namespace ClinicManagement.UI.Views.UI.Report
{
    /// <summary>
    /// Interaction logic for MedicineUsageReportView.xaml
    /// </summary>
    public partial class MedicineUsageReportView : Page
    {
        public MedicineUsageReportView()
        {
            InitializeComponent();

            // Gắn ViewModel xử lý gọi API báo cáo sử dụng thuốc (BM5.2)
            DataContext = new MedicineUsageReportViewModel();
        }
    }
}
