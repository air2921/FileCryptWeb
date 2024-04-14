namespace domain.Abstractions.Services
{
    public interface IGenerate
    {
        public string GenerateKey();
        public int GenerateSixDigitCode();
    }
}
