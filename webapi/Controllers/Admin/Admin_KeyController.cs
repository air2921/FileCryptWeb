using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/keys")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    [EntityExceptionFilter]
    public class Admin_KeyController(
        IRedisCache redisCache,
        IRepository<KeyModel> keyRepository) : ControllerBase
    {
        [HttpPut("revoke/received/{userId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> RevokeReceivedKey([FromRoute] int userId)
        {
            var keys = await keyRepository.GetByFilter(new KeysByRelationSpec(userId));
            if (keys is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            keys.received_key = null;
            await keyRepository.Update(keys);

            await redisCache.DeleteCache("receivedKey#" + userId);
            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{userId}");

            return StatusCode(200, new { message = Message.REMOVED });
        }
    }
}
