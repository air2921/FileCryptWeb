using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Offers
{
    [Route("api/admin/offers/get")]
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

        [HttpGet("one")]
        public async Task<IActionResult> ReadOneOffer(int offerID)
        {
            try
            {
                var offer = await _read.ReadById(offerID, false);

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

        [HttpGet("all/user/offers")]
        public async Task<IActionResult> ReadAllUserOffers(int userID)
        {
            try
            {
                var offers = await _dbContext.Offers.Where(o => o.sender_id == userID && o.receiver_id == userID).ToListAsync();
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
