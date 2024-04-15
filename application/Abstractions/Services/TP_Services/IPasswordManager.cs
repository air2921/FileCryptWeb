namespace application.Abstractions.Services.TP_Services
{
    public interface IPasswordManager
    {
        public string HashingPassword(string password);
        public bool CheckPassword(string InputPassword, string CorrectPassword);
    }
}
