using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    public class Admin_UserController : ControllerBase
    {
        #region fields and constructor

        private readonly IApiAdminUserService _userService;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly FileCryptDbContext _dbContext;

        public Admin_UserController(
            IApiAdminUserService userService,
            IRepository<UserModel> userRepository,
            IRepository<TokenModel> tokenRepository,
            FileCryptDbContext dbContext)
        {
            _userService = userService;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _dbContext = dbContext;
        }

        #endregion

        [HttpGet("{userId}")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetUser([FromRoute] int userId)
        {
            try
            {
                var user = await _userRepository.GetById(userId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { user });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        [XSRFProtection]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (_userService.IsHighestAdmin(target.role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                await _userRepository.Delete(userId);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("block/{userId}")]
        [XSRFProtection]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> BlockUser([FromRoute] int userId, [FromQuery] bool block)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (_userService.IsHighestAdmin(target.role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                await _userService.DbTransaction(target, block);
                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("role/{userId}")]
        [XSRFProtection]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdateRole([FromRoute] int userId, [FromQuery] string role)
        {
            try
            {
                if (_userService.IsHighestAdmin(role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (_userService.IsHighestAdmin(target.role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                target.role = role;
                await _userRepository.Update(target);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public interface IApiAdminUserService
    {
        public bool IsHighestAdmin(string role);
        public Task DbTransaction(UserModel target, bool block);
    }

    public class AdminUserService : IApiAdminUserService
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<TokenModel> _tokenRepository;

        public AdminUserService(
            FileCryptDbContext dbContext,
            IRepository<UserModel> userRepository,
            IRepository<TokenModel> tokenRepository)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
        }

        [Helper]
        public bool IsHighestAdmin(string role)
        {
            return role.Equals(Role.HighestAdmin.ToString());
        }

        [Helper]
        public async Task DbTransaction(UserModel target, bool block)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                target.is_blocked = block;
                await _userRepository.Update(target);

                if (block)
                {
                    var tokens = new List<int>();

                    var tokenModels = await _tokenRepository.GetAll(query => query.Where(t => t.user_id.Equals(target.id)));
                    foreach (var token in tokenModels)
                        tokens.Add(token.token_id);

                    await _tokenRepository.DeleteMany(tokens);
                }

                await transaction.CommitAsync();
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotDeletedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync();
                throw new EntityNotUpdatedException("Error when updating data");
            }
        }
    }
}
