namespace application.DTO
{
    public class UserDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public bool Flag2Fa { get; set; }
        public string Code { get; set; }
    }
}
