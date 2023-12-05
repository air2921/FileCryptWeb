using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Offers
{
    [Route("api/admin/offers")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadOfferController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRead<OfferModel> _read;

        public ReadOfferController(FileCryptDbContext dbContext, IRead<OfferModel> read)
        {
            _dbContext = dbContext;
            _read = read;
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> ReadOneOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _read.ReadById(offerId, false);

                return StatusCode(200, new { offer });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAllOffer()
        {
            try
            {
                var offer = await _read.ReadAll();

                return StatusCode(200, new { offer });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/offers/{userId}")]
        public async Task<IActionResult> ReadAllUserOffers([FromRoute] int userId)
        {
            try
            {
                var offers = await _dbContext.Offers
                    .Where(o => o.sender_id == userId && o.receiver_id == userId)
                    .OrderByDescending(o => o.created_at)
                    .ToListAsync();
                if (offers is null)
                    return StatusCode(404, new { message = ExceptionOfferMessages.NoOneOfferNotFound });

                return StatusCode(200, new { offers });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
