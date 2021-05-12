using System;
using System.Security.Cryptography;
using System.Text;

namespace IoTHub
{
    public class AuthenticationKeySource : IAuthenticationKeySource
    {
        private readonly string _clearTextKey;

        public AuthenticationKeySource(string encryptedKey)
        {
            if (encryptedKey == null)
            {
                throw new ArgumentNullException(nameof(encryptedKey));
            }
            _clearTextKey = UnprotectString(encryptedKey);
        }

        public string GetKey() => _clearTextKey;

        private static string UnprotectString(string protectedString)
        {
            byte[] secureBytes = Convert.FromBase64String(protectedString);
            byte[] unprotectedBytes = ProtectedData.Unprotect(secureBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(unprotectedBytes);
        }
    }
}
