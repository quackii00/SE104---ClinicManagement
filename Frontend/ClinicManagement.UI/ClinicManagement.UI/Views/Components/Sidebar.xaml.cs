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

namespace ClinicManagement.UI.Views.Components
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        public Sidebar()
        {
            InitializeComponent();
            // KHÔNG tạo MainWindowViewModel ở đây: MainWindow.xaml đã bind DataContext của Sidebar
            // về VM của cửa sổ (RelativeSource AncestorType=Window). Tạo lại sẽ dựng VM thừa.
        }
    }
}
