using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime/create")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class CreateMIMEController : ControllerBase
    {
        private readonly ICreate<FileMimeModel> _createMime;
        private readonly IInsertBase<FileMimeModel> _insertBase;

        public CreateMIMEController(ICreate<FileMimeModel> createMime, IInsertBase<FileMimeModel> insertBase)
        {
            _createMime = createMime;
            _insertBase = insertBase;
        }

        [HttpPost("allowed")]
        public async Task<IActionResult> CreateNewMime(FileMimeModel mimeModel)
        {
            await _createMime.Create(mimeModel);

            return StatusCode(201, new { message = SuccessMessage.SuccessCreatedNewMime });
        }

        [HttpPost("allowed/secure/{secure}")]
        public async Task<IActionResult> CreateSecureBaseMIMICollection([FromRoute] bool secure)
        {
            await _insertBase.DBInsertBase(null, secure);

            return StatusCode(201, new { message = SuccessMessage.SuccessMimeCollectionCreate });
        }
    }
}
