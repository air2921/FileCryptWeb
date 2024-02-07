using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;
using Newtonsoft.Json;
using webapi.Interfaces.Controllers;
using webapi.Services;
using webapi.Interfaces;

namespace webapi.Controllers.Base.CryptographyUtils
{
    public class FileService : IFileService
    {
        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();

        private readonly IRepository<FileModel> _fileRepository;
        private readonly IRepository<FileMimeModel> _mimeRepository;
        private readonly IGetSize _getSize;
        private readonly IVirusCheck _virusCheck;
        private readonly IRedisCache _redisCache;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IRepository<FileModel> fileRepository,
            IRepository<FileMimeModel> mimeRepository,
            IGetSize getSize,
            IVirusCheck virusCheck,
            IRedisCache redisCache,
            ILogger<FileService> logger)
        {
            _fileRepository = fileRepository;
            _mimeRepository = mimeRepository;
            _getSize = getSize;
            _virusCheck = virusCheck;
            _redisCache = redisCache;
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

        public async Task<bool> CheckFile(IFormFile file)
        {
            if (file.Length == 0 || file.ContentType is null)
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

        private async Task<bool> CheckMIME(string mime)
        {
            var mimes = await _redisCache.GetCachedData(Constants.MIME_COLLECTION);
            if (mimes is not null)
            {
                string[] mimesArray = JsonConvert.DeserializeObject<string[]>(mimes);
                if (mimesArray is null || mimesArray.Length.Equals(0))
                    return false;

                return mimesArray.Contains(mime);
            }
            else
            {
                var mimesDb = await _mimeRepository.GetAll();
                string[] mimesArray = mimesDb.Select(m => m.mime_name).ToArray();

                await _redisCache.CacheData(Constants.MIME_COLLECTION, mimesArray, TimeSpan.FromDays(3));

                return mimesArray.Contains(mime);
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
            await _fileRepository.Add(new FileModel
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
