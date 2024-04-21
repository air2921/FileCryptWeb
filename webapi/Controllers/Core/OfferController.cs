using application.Abstractions.Endpoints.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OfferController(
        IOfferService service,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpPost("open/{receiverId}")]
        public async Task<IActionResult> Open([FromRoute] int receiverId,
            [FromQuery] int storageId, [FromQuery] int keyId, [FromQuery] int code)
        {
            var response = await service.Add(userInfo.UserId, receiverId, keyId, storageId, code.ToString());
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("close/{offerId}")]
        public async Task<IActionResult> Close([FromRoute] int offerId, [FromQuery] string keyname,
            [FromQuery] int storageId, [FromQuery] int code)
        {
            var response = await service.Accept(userInfo.UserId, keyname, offerId, storageId, code.ToString());
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> GetOffer([FromRoute] int offerId)
        {
            var response = await service.GetOne(userInfo.UserId, offerId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { offer = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeOffers([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sent,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            var response = await service.GetRange(userInfo.UserId, skip, count, byDesc, sent, isAccepted, type);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { offer = response.ObjectData });
        }

        [HttpDelete("{offerId}")]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            var response = await service.DeleteOne(userInfo.UserId, offerId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
