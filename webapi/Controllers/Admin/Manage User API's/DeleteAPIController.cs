using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api/delete")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class DeleteAPIController : ControllerBase
    {
        private readonly IDelete<ApiModel> _deleteAPIById;
        private readonly IDeleteByName<ApiModel> _deleteAPIByName;

        public DeleteAPIController(IDelete<ApiModel> deleteAPIById, IDeleteByName<ApiModel> deleteAPIByName)
        {
            _deleteAPIById = deleteAPIById;
            _deleteAPIByName = deleteAPIByName;
        }

        [HttpDelete("revoke/apikey/{byID}")]
        public async Task<IActionResult> RevokeAPI([FromBody] ApiModel apiModel, [FromRoute] bool byID)
        {
            try
            {
                if (byID)
                {
                    await _deleteAPIById.DeleteById(apiModel.user_id);

                    return StatusCode(200, new { message = SuccessMessage.SuccessApiRevoked });
                }

                await _deleteAPIByName.DeleteByName(apiModel.api_key);

                return StatusCode(200, new { message = SuccessMessage.SuccessApiRevoked });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (FormatException)
            {
                return StatusCode(422, new { message = "Invalid route" });
            }
        }
    }
}
