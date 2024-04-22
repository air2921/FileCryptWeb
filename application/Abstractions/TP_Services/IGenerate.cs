namespace application.Abstractions.TP_Services
{
    public interface IGenerate
    {
        public string GenerateKey();
        public int GenerateSixDigitCode();
    }
}
