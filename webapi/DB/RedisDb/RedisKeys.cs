using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;

namespace webapi.DB.RedisDb
{
    public class RedisKeys : IRedisKeys
    {
        private readonly IUserInfo _userInfo;

        private string? _privatekey;
        private string? _internalKey;
        private string? _receivedKey;

        public RedisKeys(IUserInfo userInfo)
        {
            _userInfo = userInfo;
        }

        public string PrivateKey
        {
            get
            {
                return _privatekey ??= "privateKey#" + _userInfo.UserId;
            }
        }

        public string InternalKey
        {
            get
            {
                return _internalKey ??= "internalKey#" + _userInfo.UserId;
            }
        }

        public string ReceivedKey
        {
            get
            {
                return _receivedKey ??= "receivedKey#" + _userInfo.UserId;
            }
        }
    }
}
