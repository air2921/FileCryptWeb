using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/offers")]
    [ApiController]
    [Authorize]
    public class OfferController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly ICreate<OfferModel> _createOffer;
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IRead<UserModel> _readUser;
        private readonly IRead<KeyModel> _readKeys;
        private readonly IRead<OfferModel> _readOffer;
        private readonly IDelete<OfferModel> _deleteOffer;
        private readonly ITokenService _tokenService;

        public OfferController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            ICreate<OfferModel> createOffer,
            ICreate<NotificationModel> createNotification,
            IRead<UserModel> readUser,
            IRead<KeyModel> readKeys,
            IRead<OfferModel> readOffer,
            IDelete<OfferModel> deleteOffer,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _createOffer = createOffer;
            _createNotification = createNotification;
            _readUser = readUser;
            _readKeys = readKeys;
            _readOffer = readOffer;
            _deleteOffer = deleteOffer;
            _tokenService = tokenService;
        }

        [HttpPost("one")]
        public async Task<IActionResult> CreateOneOffer(int receiverID)
        {
            if (_userInfo.UserId == receiverID)
                return StatusCode(409, new { message = "Send a trade offer to yourself, are you kidding)?" });

            var receiver = await _readUser.ReadById(receiverID, null);
            if (receiver is null)
                return StatusCode(404, new { message = ExceptionUserMessages.UserNotFound });

            var keys = await _readKeys.ReadById(_userInfo.UserId, true);
            if (keys is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            if (keys.person_internal_key is null)
                return StatusCode(404, new { message = "You don't have a internal key for create an offer" });

            var offerModel = new OfferModel
            {
                offer_header = $"Proposal to accept an encryption key from a user: {_userInfo.Username}#{_userInfo.UserId}",
                offer_body = keys.person_internal_key,
                offer_type = "Encryption key trade offer",
                is_accepted = false,
                sender_id = _userInfo.UserId,
                receiver_id = receiverID
            };

            var notificationModel = new NotificationModel
            {
                message_header = "New offer",
                message = $"You got a new offer from {_userInfo.Username}#{_userInfo.UserId}",
                priority = Priority.trade.ToString(),
                send_time = DateTime.UtcNow,
                is_checked = false,
                sender_id = _userInfo.UserId,
                receiver_id = receiverID
            };

            await _createOffer.Create(offerModel);
            await _createNotification.Create(notificationModel);

            return StatusCode(201, new { offerModel });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AcceptOffer([FromRoute] int id)
        {
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.offer_id == id && o.receiver_id == _userInfo.UserId);
            if (offer is null)
                return StatusCode(404);

            var receiver = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == _userInfo.UserId);

            receiver.received_internal_key = offer.offer_body;
            offer.is_accepted = true;

            await _dbContext.SaveChangesAsync();

            return StatusCode(200, new { message = SuccessMessage.SuccessOfferAccepted });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOneOffer([FromRoute] int id)
        {
            try
            {
                var offer = await _readOffer.ReadById(id, false);

                return StatusCode(200, new { offer });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/{sended}")]
        public async Task<IActionResult> GetAll([FromRoute] bool? sended = null)
        {
            var query = _dbContext.Offers.OrderByDescending(o => o.created_at).AsQueryable();
            var offers = new List<OfferModel>();

            switch (sended)
            {
                case true:
                    offers = await query.Where(o => o.sender_id == _userInfo.UserId).ToListAsync();
                    break;

                case false:
                    offers = await query.Where(o => o.receiver_id == _userInfo.UserId).ToListAsync();
                    break;

                default:
                    offers = await query.Where(o => o.sender_id == _userInfo.UserId && o.receiver_id == _userInfo.UserId).ToListAsync();
                    break;
            }

            if (offers is null)
                return StatusCode(404, new { message = ExceptionOfferMessages.NoOneOfferNotFound });

            return StatusCode(200, new { offers });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOneOffer([FromRoute] int id)
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
