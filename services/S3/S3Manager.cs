using Amazon.S3.Model;
using application.Abstractions.TP_Services;
using domain.Exceptions;
using Microsoft.Extensions.Logging;
using services.S3.Abstractions;

namespace services.S3
{
    public class S3Manager(IS3ClientProvider s3ClientProvider, ILogger<S3Manager> logger) : IS3Manager
    {
        private readonly S3ClientDTO _provider = s3ClientProvider.GetS3Client();
        private const string ERROR = "Unknown error while receiving file";

        public async Task Upload(Stream stream, string key)
        {
            try
            {
                await _provider.s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _provider.Bucket,
                    Key = key,
                    InputStream = stream
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw new S3ClientException(ERROR);
            }
        }

        public async Task<Stream> Download(string key)
        {
            try
            {
                var response = await _provider.s3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _provider.Bucket,
                    Key = key
                });

                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw new S3ClientException(ERROR);
            }
        }

        public async Task<Dictionary<string, Stream>> DownloadCollection(IEnumerable<string> keys)
        {
            var fileStreams = new Dictionary<string, Stream>();

            var tasks = keys.Select(async key =>
            {
                try
                {
                    var stream = await Download(key);
                    fileStreams.Add(key, stream);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
            });

            await Task.WhenAll(tasks);

            return fileStreams;
        }

        public async Task Delete(string key)
        {
            try
            {
                await _provider.s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _provider.Bucket,
                    Key = key
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw new S3ClientException(ERROR);
            }
        }
    }
}
