using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class CreateMIMEController : ControllerBase
    {
        private readonly ICreate<FileMimeModel> _createMime;
        private readonly IInsertBase<FileMimeModel> _insertBase;

        public CreateMIMEController(ICreate<FileMimeModel> createMime, IInsertBase<FileMimeModel> insertBase)
        {
            _createMime = createMime;
            _insertBase = insertBase;
        }

        [HttpPost("add/new")]
        public async Task<IActionResult> CreateNewMime([FromBody] FileMimeModel mimeModel)
        {
            await _createMime.Create(mimeModel);

            return StatusCode(201, new { message = SuccessMessage.SuccessCreatedNewMime });
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateSecureBaseMIMICollection([FromQuery] bool secure)
        {
            await _insertBase.DBInsertBase(null, secure);

            return StatusCode(201, new { message = SuccessMessage.SuccessMimeCollectionCreate });
        }
    }
}
