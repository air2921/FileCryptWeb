namespace webapi.Helpers.Abstractions
{
    public interface IGenerate
    {
        public string GenerateKey();
        public int GenerateSixDigitCode();
    }
}
