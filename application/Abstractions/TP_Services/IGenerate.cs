namespace application.Abstractions.TP_Services
{
    public interface IGenerate
    {
        public string GenerateKey(int length = 32);
        public int GenerateCode(int length);
        public string GuidCombine(int count, bool useNoHyphensFormat = false);
    }
}
