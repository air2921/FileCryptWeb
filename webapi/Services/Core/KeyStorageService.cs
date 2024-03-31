using System.Security.Cryptography;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core
{
    public class KeyStorageService(
        IRepository<KeyStorageModel> storageRepository,
        [FromKeyedServices("Decrypt")] ICypherKey decryptKey,
        [FromKeyedServices("Encrypt")] ICypherKey encryptKey,
        IPasswordManager passwordManager,
        IValidation validation) : IValidator
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
