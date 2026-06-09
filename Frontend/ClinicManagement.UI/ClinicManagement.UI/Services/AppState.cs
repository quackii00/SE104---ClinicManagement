using System;

namespace ClinicManagement.UI.Services
{
    /// <summary>
    /// Quản lý trạng thái toàn bộ ứng dụng
    /// </summary>
    public class AppState
    {
        private static AppState _instance;

        public static AppState Instance
        {
            get
            {
                _instance ??= new AppState();
                return _instance;
            }
        }

        public string CurrentUserName { get; set; }
        public string CurrentUserRole { get; set; }
        public string CurrentUserEmail { get; set; }

        // Lưu email và role cuối cùng để hiển thị lại trên login form
        public string LastUsedEmail { get; set; }
        public string LastUsedRole { get; set; }

        private AppState()
        {
        }

        public void Reset()
        {
            CurrentUserName = null;
            CurrentUserRole = null;
            CurrentUserEmail = null;
            // Không reset LastUsedEmail và LastUsedRole để hiển thị lại trên form
        }

        public void SaveLastUsed()
        {
            LastUsedEmail = CurrentUserEmail;
            LastUsedRole = CurrentUserRole;
        }
    }
}
