using application.DTO.Inner;

namespace application.Abstractions.Inner
{
    public interface ITokenComparator
    {
        public string CreateJWT(JwtDTO dto);
        public string CreateRefresh();
    }
}
