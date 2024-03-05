using webapi.Security;

namespace tests.Security_Tests
{
    public class Generate_Test
    {
        [Fact]
        public void GenerateSixDigitCode_ReturnsSixDigits()
        {
            var generator = new Generate();
            var code = generator.GenerateSixDigitCode();

            Assert.InRange(code, 100000, 999999);
            Assert.Equal(6, code.ToString().Length);
        }

        [Fact]
        public void GenerateKey_ReturnsValidBase64String()
        {
            var generator = new Generate();
            var key = generator.GenerateKey();

            Assert.NotNull(key);
            Assert.NotEmpty(key);
            Assert.True(IsBase64String(key));
        }

        private bool IsBase64String(string s)
        {
            try
            {
                Convert.FromBase64String(s);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
