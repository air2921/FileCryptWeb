using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Offers
{
    [Route("api/admin/offers")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteOfferController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly IDelete<OfferModel> _deleteOffer;

        public DeleteOfferController(IUserInfo userInfo, ILogger<DeleteNotificationController> logger, IDelete<OfferModel> deleteOffer)
        {
            _userInfo = userInfo;
            _logger = logger;
            _deleteOffer = deleteOffer;
        }

        [HttpDelete("{offerId}")]
        public async Task<IActionResult> DeleteOffer([FromRoute] int offerId)
        {
            try
            {
                await _deleteOffer.DeleteById(offerId);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted offer #{offerId} from db");

                return StatusCode(200, new { message = SuccessMessage.SuccessOfferDeleted });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
