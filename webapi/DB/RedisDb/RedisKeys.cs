using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;

namespace webapi.DB.RedisDb
{
    public class RedisKeys(IUserInfo userInfo) : IRedisKeys
    {
        private string? _privatekey;
        private string? _internalKey;
        private string? _receivedKey;

        public string PrivateKey
        {
            get
            {
                return _privatekey ??= "privateKey#" + userInfo.UserId;
            }
        }

        public string InternalKey
        {
            get
            {
                return _internalKey ??= "internalKey#" + userInfo.UserId;
            }
        }

        public string ReceivedKey
        {
            get
            {
                return _receivedKey ??= "receivedKey#" + userInfo.UserId;
            }
        }
    }
}
