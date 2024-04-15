namespace application.Abstractions.Services.TP_Services
{
    public interface IGenerate
    {
        public string GenerateKey();
        public int GenerateSixDigitCode();
    }
}
