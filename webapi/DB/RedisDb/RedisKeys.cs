using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;

namespace webapi.DB.RedisDb
{
    public class RedisKeys : IRedisKeys
    {
        private readonly IUserInfo _userInfo;

        private string _privatekey;
        private string _personalInternalKey;
        private string _receivedInternalKey;

        private string _serviceFreezeFlag;
        private string _mimeCollection;

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

        public string PersonalInternalKey
        {
            get
            {
                return _personalInternalKey ??= "personalInternalKey#" + _userInfo.UserId;
            }
        }

        public string ReceivedInternalKey
        {
            get
            {
                return _receivedInternalKey ??= "receivedInternalKey#" + _userInfo.UserId;
            }
        }
    }
}
