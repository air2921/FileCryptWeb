using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL.Files.Mimes;
using webapi.Interfaces.SQL;
using webapi.Models;
using Newtonsoft.Json;
using webapi.Interfaces.Controllers;
using webapi.Localization.English;

namespace webapi.Controllers.Base.CryptographyUtils
{
    public class FileService : IFileService
    {
        private readonly IGetSize _getSize;
        private readonly IVirusCheck _virusCheck;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IReadMime _readMime;
        private readonly ICreate<FileModel> _createFile;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IGetSize getSize,
            IVirusCheck virusCheck,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IReadMime readMime,
            ICreate<FileModel> createFile,
            ILogger<FileService> logger)
        {
            _getSize = getSize;
            _virusCheck = virusCheck;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _readMime = readMime;
            _createFile = createFile;
            _logger = logger;
        }

        public bool CheckFileType(string type)
        {
            string lowerType = type.ToLowerInvariant();

            string privateType = FileType.PrivateType;
            string internalType = FileType.InternalType;
            string receivedType = FileType.ReceivedType;

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
            bool fileGood = true;

            if (file is null || file.Length == 0)
                fileGood = false;

            bool isAllowedMIME = await CheckMIME(file!.ContentType);
            bool isClean = await _virusCheck.GetResultScan(file);

            if (!isAllowedMIME || !isClean)
                fileGood = false;

            return fileGood;
        }

        public bool CheckSize(IFormFile file)
        {
            return _getSize.GetFileSizeInMb(file) < 75;
        }

        private async Task<bool> CheckMIME(string Mime)
        {
            try
            {
                var mimes = await _redisCache.GetCachedData(_redisKeys.MimeCollection);

                string[] mimesArray = JsonConvert.DeserializeObject<string[]>(mimes);

                return mimesArray.Contains(Mime);
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    var mimesDb = await _readMime.ReadAllMimes();
                    string[] mimesArray = mimesDb.ToArray();

                    var mimesJson = JsonConvert.SerializeObject(mimesArray);

                    await _redisCache.CacheData(_redisKeys.MimeCollection, mimesJson, TimeSpan.FromDays(3));

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
                if (System.IO.File.Exists(filePath))
                    await Task.Run(() => File.Delete(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
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
