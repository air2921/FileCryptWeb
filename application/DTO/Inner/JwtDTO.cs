namespace application.DTO.Inner
{
    public class JwtDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
        public TimeSpan Expires { get; set; }
    }
}
