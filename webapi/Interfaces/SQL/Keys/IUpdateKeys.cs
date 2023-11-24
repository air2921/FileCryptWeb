using webapi.Models;

namespace webapi.Interfaces.SQL.Keys
{
    public interface IUpdateKeys
    {
        Task UpdatePersonalInternalKey(int id);
        Task UpdatePersonalInternalKeyToYourOwn(KeyModel keyModel);
        Task UpdatePrivateKey(int id);
        Task CleanReceivedInternalKey(int id);
        Task UpdatePrivateKeyToYourOwn(KeyModel keyModel);
    }
}
