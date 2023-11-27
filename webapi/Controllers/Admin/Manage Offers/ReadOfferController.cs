using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Offers;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Offers
{
    [Route("api/admin/offers/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadOfferController : ControllerBase
    {
        private readonly IReadOffer _readOffer;

        public ReadOfferController(IReadOffer readOffer)
        {
            _readOffer = readOffer;
        }

        [HttpGet("one/offer")]
        public async Task<IActionResult> ReadOneOffer(int offerID)
        {
            try
            {
                var offer = await _readOffer.ReadOneOffer(offerID);

                return StatusCode(200, new { offer });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/offers")]
        public async Task<IActionResult> ReadAllUserOffers(OfferModel offerModel)
        {
            try
            {
                var offers = await _readOffer.ReadAllOffers(offerModel);

                return StatusCode(200, new { offers });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/received/offers")]
        public async Task<IActionResult> ReadReceivedOffers(int receiverID)
        {
            try
            {
                var receivedOffers = await _readOffer.ReadAllReceivedOffers(receiverID);

                return StatusCode(200, new { receivedOffers });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/sended/offers")]
        public async Task<IActionResult> ReadSendedOffers(int senderID)
        {
            try
            {
                var sendedOffers = await _readOffer.ReadAllSendedOffers(senderID);

                return StatusCode(200, new { sendedOffers });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
