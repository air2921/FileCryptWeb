using webapi.Interfaces.Services;

namespace webapi.Helpers
{
    public class Validation : IValidation
    {
        public const string EncryptionKey = "^[A-Za-z0-9+/]{43}=$";
        public const string Password = "^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,30}$";
        public const string Email = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";
        public const string Username = "^(?!.*#)[\\s\\S]{1,30}$";

        public bool IsBase64String(string? key)
        {
            if (string.IsNullOrEmpty(key) || key.Length % 4 != 0)
                return false;

            try
            {
                return Convert.FromBase64String(key).Length.Equals(32);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public bool IsSixDigit(int value)
        {
            return value >= 100000 && value <= 999999;
        }
    }
}
