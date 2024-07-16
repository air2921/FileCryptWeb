using application.Master_Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/offer")]
    [ApiController]
    [Authorize(Policy = "RequireAdminPolicy")]
    public class _OfferController(Admin_OfferService service) : ControllerBase
    {
        [HttpGet("{offerId}")]
        public async Task<IActionResult> GetOffer([FromRoute] int offerId)
        {
            var response = await service.GetOne(offerId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { offer = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeOffers([FromQuery] int? userId, [FromQuery] int skip,
            [FromQuery] int count, [FromQuery] bool byDesc, [FromQuery] bool? sent,
            [FromQuery] bool? isAccepted, [FromQuery] int? type)
        {
            var response = await service.GetRange(userId, skip, count, byDesc, sent, isAccepted, type);
            return StatusCode(response.Status, new { offers = response.ObjectData });
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            var response = await service.DeleteOne(offerId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeOffers([FromBody] IEnumerable<int> identifiers)
        {
            var response = await service.DeleteRange(identifiers);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
