using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminTokenService
    {
        Task<Response> RevokeAllUserTokens(int targetId, string ownRole);
        Task<Response> RevokeToken(int tokenId, string ownRole);
    }
}
