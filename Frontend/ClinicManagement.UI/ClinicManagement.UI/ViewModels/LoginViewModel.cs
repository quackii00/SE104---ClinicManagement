using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.DTOs;
using ClinicManagement.UI.Services;

namespace ClinicManagement.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = new AuthService();

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
            ErrorMessage = string.Empty;

            try
            {
                // Gọi API đăng nhập THẬT để lấy JWT (POST api/auth/login)
                var result = await _authService.LoginAsync(new LoginRequest
                {
                    Email = Email,
                    Password = Password
                });

                // Server trả 401 => sai thông tin đăng nhập
                if (result == null)
                {
                    ErrorMessage = "Email hoặc mật khẩu không đúng.";
                    return;
                }

                // Lưu JWT + thông tin người dùng vào kho dùng chung (AppState).
                // Từ đây BaseApiService sẽ tự đính kèm Bearer token cho mọi request về sau.
                AppState.Instance.AuthToken = result.Token;
                AppState.Instance.CurrentUserEmail = Email;
                AppState.Instance.CurrentUserName =
                    string.IsNullOrWhiteSpace(result.HoTen) ? result.TenDangNhap : result.HoTen;
                AppState.Instance.CurrentUserRole = result.VaiTro; // tên hiển thị tiếng Việt (vd "Bác Sĩ")

                // Lưu email để gợi ý lại lần đăng nhập sau
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
                // Lỗi mạng / Backend chưa chạy / sai chứng chỉ HTTPS...
                ErrorMessage = "Không kết nối được máy chủ. Hãy kiểm tra Backend đang chạy rồi thử lại.";
                System.Diagnostics.Debug.WriteLine($"[Login Error]: {ex}");
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
