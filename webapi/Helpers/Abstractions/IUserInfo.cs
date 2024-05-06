namespace webapi.Helpers.Abstractions
{
    public interface IUserInfo
    {
        public int UserId { get; }
        public string Username { get; }
        public string Role { get; }
        public string Email { get; }
        public string RequestId { get; }
    }
}
