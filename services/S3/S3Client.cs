using Amazon.S3;
using Microsoft.Extensions.Configuration;
using services.S3.Abstractions;

namespace services.S3
{
    public class S3Client(IConfiguration configuration) : IS3ClientProvider
    {
        public S3ClientDTO GetS3Client()
        {
            var keyId = configuration.GetSection("S3")["keyId"]!;
            var accessKey = configuration.GetSection("S3")["accessKey"]!;
            var bucket = configuration.GetSection("S3")["bucket"]!;

            return new S3ClientDTO
            {
                Bucket = bucket,
                s3Client = new AmazonS3Client(keyId, accessKey, new AmazonS3Config
                {
                    ServiceURL = "https://s3.yandexcloud.net"
                })
            };
        }
    }
}
