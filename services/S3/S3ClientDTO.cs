using Amazon.S3;

namespace services.S3
{
    public class S3ClientDTO
    {
        public string Bucket { get; set; }
        public AmazonS3Client s3Client { get; set; }
    }
}
