using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.Views.UI.Dashboard;

namespace ClinicManagement.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _userRole;
        private string _userName;
        private object _currentView;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserRole
        {
            get { return _userRole; }
            set
            {
                if (_userRole != value)
                {
                    _userRole = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                }
            }
        }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LogoutCommand { get; }

        public MainWindowViewModel()
        {
            // Lấy dữ liệu từ AppState
            UserRole = AppState.Instance.CurrentUserRole;
            UserName = AppState.Instance.CurrentUserName ?? "Đang tải..."; // Sẽ được cập nhật từ backend

            LogoutCommand = new RelayCommand(Logout);

            // Mặc định mở Dashboard
            CurrentView = new Dashboard();
        }

        private void Logout(object parameter)
        {
            // Reset AppState
            AppState.Instance.Reset();

            // Ẩn MainWindow (không đóng)
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.Hide();
                    break;
                }
            }

            // Mở LoginWindow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
