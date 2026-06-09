using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _email;
        private string _password;
        private string _selectedRole;
        private bool _isLoading;
        private string _errorMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedRole
        {
            get { return _selectedRole; }
            set
            {
                if (_selectedRole != value)
                {
                    _selectedRole = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SignInCommand { get; }

        public ObservableCollection<string> Roles { get; set; }

        public LoginViewModel()
        {
            Roles = new ObservableCollection<string>
            {
                "Tiếp Tân",
                "Kế Toán",
                "Admin",
                "Bác Sĩ"
            };

            SignInCommand = new RelayCommand(SignIn, CanSignIn);
            _selectedRole = "Bác Sĩ";

            // Lấy email và role cuối cùng nếu có
            if (!string.IsNullOrEmpty(AppState.Instance.LastUsedEmail))
            {
                Email = AppState.Instance.LastUsedEmail;
            }
            if (!string.IsNullOrEmpty(AppState.Instance.LastUsedRole))
            {
                SelectedRole = AppState.Instance.LastUsedRole;
            }
        }

        public void OnEmailLostFocus()
        {
            ValidateEmail();
        }

        private bool CanSignIn(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   IsValidEmail(Email) &&
                   !IsLoading;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = string.Empty;
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ. Vui lòng nhập email đúng định dạng (ví dụ: user@example.com)";
            }
            else
            {
                ErrorMessage = string.Empty;
            }
        }

        private void SignIn(object parameter)
        {
            ErrorMessage = string.Empty;

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ. Vui lòng nhập email đúng định dạng (ví dụ: user@example.com)";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập password";
                return;
            }

            // Chạy async để không block UI
            _ = SignInAsync();
        }

        private async Task SignInAsync()
        {
            IsLoading = true;

            try
            {
                // Giả lập delay loading (2 giây)
                await Task.Delay(2000);

                // TODO: Implement authentication logic here
                // This is where you would call your authentication service

                System.Diagnostics.Debug.WriteLine($"Attempting login with Email: {Email}, Role: {SelectedRole}");

                // Lưu thông tin đăng nhập vào AppState
                AppState.Instance.CurrentUserEmail = Email;
                AppState.Instance.CurrentUserRole = SelectedRole;

                // Lưu email và role để hiển thị lại lần tới
                AppState.Instance.SaveLastUsed();

                // Đăng nhập thành công - mở MainWindow và đóng LoginWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Lấy LoginWindow hiện tại và đóng nó
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is LoginWindow loginWindow)
                    {
                        loginWindow.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
