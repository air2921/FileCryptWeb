using webapi.Helpers;

namespace tests.Helpers_Tests
{
    public class Validation_Test
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid_base64_string")]
        [InlineData("123456789012345678901234567890123456789012345678901234567890")]
        public void IsBase64String_InvalidStrings_ReturnsFalse(string input)
        {
            var validation = new Validation();
            var result = validation.IsBase64String(input);

            Assert.False(result);
        }

        [Fact]
        public void IsBase64String_ValidStrings_ReturnsTrue()
        {
            var validation = new Validation();
            var result = validation.IsBase64String("8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8=");

            Assert.True(result);
        }

        [Fact]
        public void IsSixDigit_ValidValues_ReturnsTrue()
        {
            var validation = new Validation();
            var result = validation.IsSixDigit(999999);

            Assert.True(result);
        }

        [Theory]
        [InlineData(9999)]
        [InlineData(1000000)]
        [InlineData(999)]
        [InlineData(10000000)]
        public void IsSixDigit_InvalidValues_ReturnsFalse(int value)
        {
            var validation = new Validation();
            var result = validation.IsSixDigit(value);

            Assert.False(result);
        }
    }
}
