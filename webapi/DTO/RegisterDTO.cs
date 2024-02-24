namespace webapi.DTO
{
    public class RegisterDTO
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public bool is_2fa_enabled { get; set; }
    }
}
