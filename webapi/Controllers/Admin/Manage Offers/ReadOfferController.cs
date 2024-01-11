﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.DB;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Services;
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
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly IRead<OfferModel> _read;

        public ReadOfferController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            ILogger<DeleteNotificationController> logger,
            IRead<OfferModel> read)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _logger = logger;
            _read = read;
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> ReadOneOffer([FromRoute] int offerId)
        {
            try
            {
                var offer = await _read.ReadById(offerId, false);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested offer information #{offerId}");

                return StatusCode(200, new { offer });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAllOffer([FromQuery] int skip, [FromQuery] int count)
        {
            try
            {
                var offer = await _read.ReadAll(skip, count);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested information about offers, skipped {skip} and quantity requested {count}");

                return StatusCode(200, new { offer });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> ReadAllUserOffers([FromRoute] int userId)
        {
            try
            {
                var offers = await _dbContext.Offers
                    .Where(o => o.sender_id == userId || o.receiver_id == userId)
                    .OrderByDescending(o => o.created_at)
                    .ToListAsync();

                if (offers is null)
                    return StatusCode(404, new { message = ExceptionOfferMessages.NoOneOfferNotFound });

                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about all offers user#{userId}");

                return StatusCode(200, new { offers });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
