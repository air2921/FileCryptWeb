using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Core
{
    [Route("api/core/offer")]
    [ApiController]
    [Authorize]
    public class OfferController : ControllerBase
    {

    }
}
