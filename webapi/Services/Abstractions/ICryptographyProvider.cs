using Microsoft.AspNetCore.Mvc;
using webapi.Services.Core;

namespace webapi.Services.Abstractions
{
    public interface ICryptographyProvider
    {
        public Task<CryptographyParams> GetCryptographyParams(string fileType, string operation);
        public Task<IActionResult> EncryptFile(CryptographyOperationOptions options);
    }
}
