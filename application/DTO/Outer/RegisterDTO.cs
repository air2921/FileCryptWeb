namespace application.DTO.Outer
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Is_2fa_enabled { get; set; }
    }
}
