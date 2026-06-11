using System.Windows;
using System.Windows.Controls;
using ClinicManagement.UI.ViewModels;

namespace ClinicManagement.UI.Views.UI.Auth
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox passwordBox)
            {
                vm.Password = passwordBox.Password;
            }
        }

    }
}