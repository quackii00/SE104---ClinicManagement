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

namespace ClinicManagement.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext (MainWindowViewModel) được tạo DUY NHẤT 1 lần trong MainWindow.xaml (<Window.DataContext>).
            // Không tạo lại ở đây để tránh dựng nhiều ViewModel và gọi API khởi động trùng lặp.
        }
    }
}