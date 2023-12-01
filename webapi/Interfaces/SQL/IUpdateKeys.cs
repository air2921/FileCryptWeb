using webapi.Models;

namespace webapi.Interfaces.SQL
{
    public interface IUpdateKeys
    {
        Task UpdatePersonalInternalKey(KeyModel keyModel);
        Task UpdatePrivateKey(KeyModel keyModel);
        Task CleanReceivedInternalKey(int id);
    }
}
