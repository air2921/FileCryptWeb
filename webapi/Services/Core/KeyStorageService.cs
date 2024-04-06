using System.Text.RegularExpressions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Services.Abstractions;

namespace webapi.Services.Core
{
    public class KeyStorageService(IValidation validation) : IValidator
    {
        public bool IsValid(object data, object parameter = null)
        {
            if (data is not string)
                return false;

            string key = (string)data;
            return !string.IsNullOrWhiteSpace(key) && validation.IsBase64String(key) && Regex.IsMatch(key, Validation.EncryptionKey);
        }
    }
}
