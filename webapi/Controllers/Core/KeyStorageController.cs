using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Core
{
    [Route("api/core/keys/storage")]
    [ApiController]
    [Authorize]
    public class KeyStorageController : ControllerBase
    {

    }
}
