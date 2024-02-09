using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/offers")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_OfferController : ControllerBase
    {
        private readonly IRepository<OfferModel> _offerRepository;
        private readonly ISorting _sorting;

        public Admin_OfferController(IRepository<OfferModel> offerRepository, ISorting sorting)
        {
            _offerRepository = offerRepository;
            _sorting = sorting;
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> GetOffer([FromRoute] int offerId)
        {
            var offer = await _offerRepository.GetById(offerId);
            if (offer is null)
                return StatusCode(404);

            return StatusCode(200, new { offer });
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetRangeOffers([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? sended,
            [FromQuery] bool? isAccepted, [FromQuery] string? type)
        {
            return StatusCode(200, new { offers = await _offerRepository.GetAll(_sorting.SortOffers(userId, skip, count, byDesc, sended, isAccepted, type)) });
        }

        [HttpDelete("{offerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            try
            {
                await _offerRepository.Delete(offerId);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeOffers([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await _offerRepository.DeleteMany(identifiers);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
