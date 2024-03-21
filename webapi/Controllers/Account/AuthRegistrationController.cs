using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.DB;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    public class AuthRegistrationController : ControllerBase
    {
        private readonly string USER_OBJECT = "AuthRegistrationController_UserObject_Email:";

        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<AuthRegistrationController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IConfiguration _configuration;
        private readonly IValidation _validation;
        private readonly IGenerate _generate;

        public AuthRegistrationController(
            IRepository<UserModel> userRepository,
            IRegistrationService registrationService,
            ILogger<AuthRegistrationController> logger,
            IPasswordManager passwordManager,
            IConfiguration configuration,
            IValidation validation,
            IGenerate generate)
        {
            _userRepository = userRepository;
            _registrationService = registrationService;
            _logger = logger;
            _passwordManager = passwordManager;
            _configuration = configuration;
            _validation = validation;
            _generate = generate;
        }

        #endregion

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO userDTO)
        {
            try
            {
                userDTO.email = userDTO.email.ToLowerInvariant();
                int code = _generate.GenerateSixDigitCode();

                if (!_registrationService.IsValidData(userDTO))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(userDTO.email)));
                if (user is not null)
                    return StatusCode(409, new { message = Message.USER_EXISTS });

                await _registrationService.SendMessage(userDTO.username, userDTO.email, code);
                await _registrationService.SetUser($"{USER_OBJECT}{userDTO.email}", new UserObject
                {
                    Email = userDTO.email,
                    Password = userDTO.password,
                    Username = userDTO.username,
                    Role = Role.User.ToString(),
                    Flag2Fa = userDTO.is_2fa_enabled,
                    Code = code.ToString()
                });

                return StatusCode(200, new { message = Message.EMAIL_SENT });
            }
            catch (SmtpClientException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("verify")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> VerifyAccount([FromQuery] int code, [FromQuery] string email)
        {
            try
            {
                var user = await _registrationService.GetUser($"{USER_OBJECT}{email.ToLowerInvariant()}");
                if (user is null)
                    return StatusCode(404, new { message = Message.TASK_TIMED_OUT });

                bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), user.Code);
                if (!IsCorrect)
                    return StatusCode(422, new { message = Message.INCORRECT });

                await _registrationService.RegisterTransaction(user);

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public interface IRegistrationService
    {
        public bool IsValidData(RegisterDTO userDTO);
        public Task RegisterTransaction(UserObject user);
        public Task SendMessage(string username, string email, int code);
        public Task SetUser(string key, UserObject user);
        public Task<UserObject> GetUser(string key);
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly IDatabaseTransaction _transaction;
        private readonly IConfiguration _configuration;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRedisCache _redisCache;
        private readonly IGenerate _generate;
        private readonly IPasswordManager _passwordManager;
        private readonly IEmailSender _emailSender;
        private readonly ICypherKey _encrypt;
        private readonly byte[] secretKey;

        public RegistrationService(
            IDatabaseTransaction transaction,
            IConfiguration configuration,
            IRepository<UserModel> userRepository,
            IRepository<KeyModel> keyRepository,
            IRedisCache redisCache,
            IGenerate generate,
            IPasswordManager passwordManager,
            IEmailSender emailSender,
            [FromKeyedServices("Encrypt")] ICypherKey encrypt)
        {
            _transaction = transaction;
            _configuration = configuration;
            _userRepository = userRepository;
            _keyRepository = keyRepository;
            _redisCache = redisCache;
            _generate = generate;
            _passwordManager = passwordManager;
            _emailSender = emailSender;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        [Helper]
        public bool IsValidData(RegisterDTO userDTO)
        {
            bool isValidUsername = Regex.IsMatch(userDTO.username, Validation.Username);
            bool isValidPassword = Regex.IsMatch(userDTO.password, Validation.Password);

            return isValidUsername && isValidPassword;
        }

        [Helper]
        public async Task RegisterTransaction(UserObject user)
        {
            try
            {
                var id = await _userRepository.Add(new UserModel
                {
                    email = user.Email,
                    password = user.Password,
                    username = user.Username,
                    role = user.Role,
                    is_2fa_enabled = user.Flag2Fa,
                    is_blocked = false
                }, e => e.id);

                await _keyRepository.Add(new KeyModel
                {
                    user_id = id,
                    private_key = await _encrypt.CypherKeyAsync(_generate.GenerateKey(), secretKey)
                });

                await _transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
            }
        }

        [Helper]
        public async Task SendMessage(string username, string email, int code)
        {
            try
            {
                await _emailSender.SendMessage(new EmailDto
                {
                    username = username,
                    email = email,
                    subject = EmailMessage.VerifyEmailHeader,
                    message = EmailMessage.VerifyEmailBody + code
                });
            }
            catch (SmtpClientException)
            {
                throw;
            }
        }

        [Helper]
        public async Task SetUser(string key, UserObject user)
        {
            user.Password = _passwordManager.HashingPassword(user.Password);
            user.Code = _passwordManager.HashingPassword(user.Code);

            await _redisCache.CacheData(key, user, TimeSpan.FromMinutes(10));
        }

        [Helper]
        public async Task<UserObject> GetUser(string key)
        {
            var userObject = await _redisCache.GetCachedData(key);
            if (userObject is not null)
                return JsonConvert.DeserializeObject<UserObject>(userObject);
            else
                return null;
        }
    }

    [AuxiliaryObject]
    public class UserObject
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public bool Flag2Fa { get; set; }
        public string Code { get; set; }
    }
}
