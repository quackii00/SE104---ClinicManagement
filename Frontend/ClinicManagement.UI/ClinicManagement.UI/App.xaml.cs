using System;
using System.Windows;
using ClinicManagement.UI.Services;
using ClinicManagement.UI.Views;

namespace ClinicManagement.UI
{
    public partial class App : Application
    {
        private readonly TokenStorageService _tokenStorageService = new TokenStorageService();

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // (NFR Độ tin cậy) Cảnh báo rõ ràng cho người dùng khi mất kết nối máy chủ,
            // thay vì để màn hình hiển thị trống/0 mà không báo gì.
            BaseApiService.ConnectionError += message =>
            {
                Current?.Dispatcher.Invoke(() =>
                    MessageBox.Show(message, "Mất kết nối máy chủ",
                        MessageBoxButton.OK, MessageBoxImage.Warning));
            };

            try
            {
                string savedToken = _tokenStorageService.GetToken();

                if (!string.IsNullOrEmpty(savedToken))
                {
                    AppState.Instance.AuthToken = savedToken;
                    AppState.Instance.CurrentUserName = _tokenStorageService.GetName();
                    AppState.Instance.CurrentUserRole = _tokenStorageService.GetRole();
                    AppState.Instance.CurrentUserRoleCode = _tokenStorageService.GetRoleCode();

                    var mainWindow = new MainWindow();
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App Startup] Lỗi tự động đăng nhập: {ex.Message}");
                _tokenStorageService.ClearToken();
            }

            var loginWindow = new LoginWindow();
            this.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}