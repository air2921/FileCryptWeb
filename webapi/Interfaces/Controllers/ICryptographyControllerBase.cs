using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Base;

namespace webapi.Interfaces.Controllers
{
    public interface ICryptographyControllerBase
    {
        public Task<IActionResult> EncryptFile(CryptographyOperationOptions options);
    }
}
