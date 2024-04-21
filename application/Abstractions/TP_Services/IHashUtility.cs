namespace application.Abstractions.TP_Services
{
    public interface IHashUtility
    {
        public string Hash(string password);
        public bool Verify(string input, string src);
    }
}
