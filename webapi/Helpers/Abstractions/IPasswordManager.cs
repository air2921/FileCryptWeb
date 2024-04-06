namespace webapi.Helpers.Abstractions
{
    public interface IPasswordManager
    {
        public string HashingPassword(string password);
        public bool CheckPassword(string InputPassword, string CorrectPassword);
    }
}
