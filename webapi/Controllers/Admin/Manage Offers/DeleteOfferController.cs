using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Offers
{
    [Route("api/admin/offers/delete")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteOfferController : ControllerBase
    {
        private readonly IDelete<OfferModel> _deleteOffer;

        public DeleteOfferController(IDelete<OfferModel> deleteOffer)
        {
            _deleteOffer = deleteOffer;
        }

        [HttpDelete("one")]
        public async Task<IActionResult> DeleteOffer([FromBody] int id)
        {
            try
            {
                await _deleteOffer.DeleteById(id);

                return StatusCode(200, new { message = SuccessMessage.SuccessOfferDeleted });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
