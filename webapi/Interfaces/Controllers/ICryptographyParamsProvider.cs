using webapi.Controllers.Base;

namespace webapi.Interfaces.Controllers
{
    public interface ICryptographyParamsProvider
    {
        public Task<CryptographyParams> GetCryptographyParams(string fileType);
    }
}
