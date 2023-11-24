using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL.Keys;
using webapi.Localization.English;

namespace webapi.Controllers.Base.CryptographyUtils
{
    public class CryptographyParamsProvider : ICryptographyParamsProvider
    {
        private readonly string privateType = FileType.PrivateType.ToLowerInvariant();
        private readonly string internalType = FileType.InternalType.ToLowerInvariant();
        private readonly string receivedType = FileType.ReceivedType.ToLowerInvariant();

        private readonly IUserInfo _userInfo;
        private readonly ILogger<CryptographyParamsProvider> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IReadKeys _readKeys;

        public CryptographyParamsProvider(
            IUserInfo userInfo,
            ILogger<CryptographyParamsProvider> logger,
            IRedisKeys redisKeys,
            IRedisCache redisCache,
            IReadKeys readKeys)
        {
            _userInfo = userInfo;
            _logger = logger;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _readKeys = readKeys;
        }

        public async Task<CryptographyParams> GetCryptographyParams(string fileType)
        {
            string lowerFileType = fileType.ToLowerInvariant();

            try
            {
                if (lowerFileType == privateType)
                {
                    return new CryptographyParams(await _redisCache.CacheKey(_redisKeys.PrivateKey, () => _readKeys.ReadPrivateKey(_userInfo.UserId)));
                }
                else if (lowerFileType == internalType)
                {
                    return new CryptographyParams(await _redisCache.CacheKey(_redisKeys.PersonalInternalKey, () => _readKeys.ReadPersonalInternalKey(_userInfo.UserId)));
                }
                else if (lowerFileType == receivedType)
                {
                    return new CryptographyParams(await _redisCache.CacheKey(_redisKeys.ReceivedInternalKey, () => _readKeys.ReadReceivedInternalKey(_userInfo.UserId)));
                }
                throw new InvalidRouteException();
            }
            catch (UserException)
            {
                throw;
            }
            catch (KeyException)
            {
                throw;
            }
        }
    }
}
