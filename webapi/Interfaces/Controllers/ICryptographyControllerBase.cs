using Microsoft.AspNetCore.Mvc;
using webapi.Cryptography;

namespace webapi.Interfaces.Controllers
{
    public interface ICryptographyControllerBase
    {
        public Task<IActionResult> EncryptFile(
            Func<string, byte[], string, CancellationToken, Task<CryptographyResult>> CryptographyFunction,
            string key, IFormFile file,
            int userID, string type, string operation);
    }
}
