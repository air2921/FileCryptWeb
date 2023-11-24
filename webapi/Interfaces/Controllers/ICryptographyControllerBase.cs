using Microsoft.AspNetCore.Mvc;
using webapi.Services.Cryptography;

namespace webapi.Interfaces.Controllers
{
    public interface ICryptographyControllerBase
    {
        public Task<IActionResult> EncryptFile(
            Func<string, byte[], CancellationToken, Task<CryptographyResult>> CryptographyFunction,
            string key, IFormFile file,
            int userID, string type);
    }
}
