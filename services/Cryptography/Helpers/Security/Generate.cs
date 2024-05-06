using System.Security.Cryptography;
using System.Text;
using application.Abstractions.TP_Services;

namespace services.Cryptography.Helpers.Security
{
    public class Generate : IGenerate
    {
        public int GenerateCode(int length)
        {
            var rnd = new Random();

            if (length <= 0 || length >= 11)
                throw new NotSupportedException("Invalid code length");

            var builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                builder.Append(rnd.Next(10));

            return int.Parse(builder.ToString());
        }

        public string GenerateKey(int length = 32)
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] key = new byte[length];
            rng.GetBytes(key);

            return Convert.ToBase64String(key);
        }

        public string GuidCombine(int count, bool useNoHyphensFormat = false)
        {
            if (count.Equals(0) || count >= 11)
                throw new NotSupportedException("Too long Guid");

            var builder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if (useNoHyphensFormat)
                    builder.Append(Guid.NewGuid().ToString("N"));
                else
                    builder.Append(Guid.NewGuid().ToString());
            }

            return builder.ToString();
        }
    }
}
