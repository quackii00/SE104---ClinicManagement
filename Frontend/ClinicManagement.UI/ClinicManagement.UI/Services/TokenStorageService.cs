using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ClinicManagement.UI.Services
{
    public class TokenStorageService
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ClinicManagement_Secret_23521698");
        private readonly string _tokenFilePath;

        public class UserSession
        {
            public string Token { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public string RoleCode { get; set; }
        }

        public TokenStorageService()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClinicManagement");
            Directory.CreateDirectory(appDataFolder);
            _tokenFilePath = Path.Combine(appDataFolder, "user_session.dat");
        }

        private UserSession LoadSession()
        {
            try
            {
                if (!File.Exists(_tokenFilePath)) return null;

                byte[] encryptedData = File.ReadAllBytes(_tokenFilePath);
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);
                string json = Encoding.UTF8.GetString(decryptedData);

                return JsonSerializer.Deserialize<UserSession>(json);
            }
            catch (Exception)
            {
                ClearToken();
                return null;
            }
        }

        private void SaveSession(UserSession session)
        {
            try
            {
                string json = JsonSerializer.Serialize(session);
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(json);
                byte[] encryptedData = ProtectedData.Protect(dataToEncrypt, Entropy, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(_tokenFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TokenStorage] Lỗi lưu session: {ex.Message}");
            }
        }

        public void SaveToken(string token)
        {
            var session = LoadSession() ?? new UserSession();
            session.Token = token;
            SaveSession(session);
        }

        public string GetToken()
        {
            return LoadSession()?.Token;
        }

        public void SaveName(string name)
        {
            var session = LoadSession() ?? new UserSession();
            session.Name = name;
            SaveSession(session);
        }

        public string GetName()
        {
            return LoadSession()?.Name;
        }

        public void SaveRole(string role)
        {
            var session = LoadSession() ?? new UserSession();
            session.Role = role;
            SaveSession(session);
        }

        public string GetRole()
        {
            return LoadSession()?.Role;
        }

        public void SaveRoleCode(string roleCode)
        {
            var session = LoadSession() ?? new UserSession();
            session.RoleCode = roleCode;
            SaveSession(session);
        }

        public string GetRoleCode()
        {
            return LoadSession()?.RoleCode;
        }

        public void ClearToken()
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
            }
        }
    }
}