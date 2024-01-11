using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;
using Newtonsoft.Json;
using webapi.Interfaces.Controllers;
using webapi.Services;

namespace webapi.Controllers.Base.CryptographyUtils
{
    public class FileService : IFileService
    {
        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();

        private readonly IGetSize _getSize;
        private readonly IVirusCheck _virusCheck;
        private readonly IRedisCache _redisCache;
        private readonly IRead<FileMimeModel> _read;
        private readonly ICreate<FileModel> _createFile;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IGetSize getSize,
            IVirusCheck virusCheck,
            IRedisCache redisCache,
            IRead<FileMimeModel> read,
            ICreate<FileModel> createFile,
            ILogger<FileService> logger)
        {
            _getSize = getSize;
            _virusCheck = virusCheck;
            _redisCache = redisCache;
            _read = read;
            _createFile = createFile;
            _logger = logger;
        }

        public bool CheckFileType(string type)
        {
            string lowerType = type.ToLowerInvariant();

            string[] typesArray = new string[]
            {
                privateType,
                internalType,
                receivedType
            };

            return typesArray.Contains(lowerType);
        }

        public async Task<bool> CheckFile(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return false;

            if (file.ContentType is null)
                return false;

            if (!await _virusCheck.GetResultScan(file))
                return false;

            if (!await CheckMIME(file.ContentType))
                return false;

            return true;
        }

        public bool CheckSize(IFormFile file)
        {
            return _getSize.GetFileSizeInMb(file) < 75;
        }

        private async Task<bool> CheckMIME(string Mime)
        {
            try
            {
                var mimes = await _redisCache.GetCachedData(Constants.MIME_COLLECTION);

                string[] mimesArray = JsonConvert.DeserializeObject<string[]>(mimes);

                return mimesArray.Contains(Mime);
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    var mimesDb = await _read.ReadAll(0, 10000);
                    string[] mimesArray = mimesDb.Select(m => m.mime_name).ToArray();

                    var mimesJson = JsonConvert.SerializeObject(mimesArray);

                    await _redisCache.CacheData(Constants.MIME_COLLECTION, mimesJson, TimeSpan.FromDays(3));

                    return mimesArray.Contains(Mime);
                }
                catch (MimeException)
                {
                    return false;
                }
            }
        }

        public async Task UploadFile(string filePath, IFormFile file)
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    await Task.Run(() => File.Delete(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public async Task CreateFile(int userID, string uniqueFileName, string mime, string fileType)
        {
            await _createFile.Create(new FileModel
            {
                user_id = userID,
                file_name = uniqueFileName,
                file_mime = mime,
                operation_date = DateTime.UtcNow,
                type = fileType,
            });
        }
    }
}
