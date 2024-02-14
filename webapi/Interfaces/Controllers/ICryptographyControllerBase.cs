using Microsoft.AspNetCore.Mvc;
using webapi.Cryptography;

namespace webapi.Interfaces.Controllers
{
    public interface ICryptographyControllerBase
    {
        public Task<IActionResult> EncryptFile(
            Func<string, byte[], CancellationToken, string, Task<CryptographyResult>> CryptographyFunction,
            string key, IFormFile file,
            int userID, string type, string operation);
    }
}
