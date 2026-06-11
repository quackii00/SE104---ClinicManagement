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

namespace ClinicManagement.UI.Views.UI.Update
{
    /// <summary>
    /// Interaction logic for UpdateRegulations.xaml
    /// </summary>
    public partial class UpdateRegulations : UserControl
    {
        public UpdateRegulations()
        {
            InitializeComponent();

            // Gắn ViewModel để xử lý nạp/gửi dữ liệu cập nhật quy định qua QuyDinhService
            DataContext = new UpdateRegulationsViewModel();
        }
    }
}
