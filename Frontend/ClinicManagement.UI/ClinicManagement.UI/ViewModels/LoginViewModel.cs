using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.DTOs;

namespace ClinicManagement.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _email;
        private string _password;
        private string _selectedRole = "Bác Sĩ";
        private bool _isLoading;
        private string _errorMessage;
        private bool _isRememberMe;

        private readonly TokenStorageService _tokenStorageService = new TokenStorageService();

        public event PropertyChangedEventHandler PropertyChanged;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsRememberMe
        {
            get => _isRememberMe;
            set { _isRememberMe = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "Tiếp Tân", "Kế Toán", "Admin", "Bác Sĩ" };

        public ICommand SignInCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            SignInCommand = new RelayCommand(async o => await SignInAsync(), o => !IsLoading);
            _email = AppState.Instance.LastUsedEmail;
        }

        public void OnEmailLostFocus()
        {
            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            {
                ErrorMessage = "Định dạng Email không hợp lệ!";
            }
            else
            {
                ErrorMessage = string.Empty;
            }
        }

        private async Task SignInAsync()
        {
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                bool success = await _authService.LoginAsync(Email, Password);
                if (success)
                {
                    if (!string.Equals(AppState.Instance.CurrentUserRole, SelectedRole, StringComparison.OrdinalIgnoreCase))
                    {
                        ErrorMessage = "Vai trò tài khoản không khớp!";
                        IsLoading = false;
                        return;
                    }

                    if (IsRememberMe)
                    {
                        _tokenStorageService.SaveToken(AppState.Instance.AuthToken);
                        _tokenStorageService.SaveName(AppState.Instance.CurrentUserName);
                        _tokenStorageService.SaveRole(AppState.Instance.CurrentUserRole);
                        _tokenStorageService.SaveRoleCode(AppState.Instance.CurrentUserRoleCode);
                    }
                    else
                    {
                        _tokenStorageService.ClearToken();
                        _tokenStorageService.SaveName(string.Empty);
                        _tokenStorageService.SaveRole(string.Empty);
                        _tokenStorageService.SaveRoleCode(string.Empty);
                    }

                    AppState.Instance.LastUsedEmail = Email;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.GetType().FullName.Contains("Login") || window is LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Email hoặc mật khẩu không chính xác.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối Server: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}