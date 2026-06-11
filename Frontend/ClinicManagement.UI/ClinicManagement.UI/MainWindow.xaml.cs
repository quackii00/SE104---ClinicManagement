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
        // Trong MainWindow.xaml.cs
        public MainWindow()
        {
            InitializeComponent();
            // Tạo duy nhất 1 instance và gán cho nó
            this.DataContext = new MainWindowViewModel();
        }
    }
}