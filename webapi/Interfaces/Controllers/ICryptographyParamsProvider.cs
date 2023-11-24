using webapi.Controllers.Base.CryptographyUtils;

namespace webapi.Interfaces.Controllers
{
    public interface ICryptographyParamsProvider
    {
        public Task<CryptographyParams> GetCryptographyParams(string fileType);
    }
}
