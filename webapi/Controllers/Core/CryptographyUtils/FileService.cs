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
        private const int TASK_AWAITING = 10000;

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
            try
            {
                if (file.Length == 0 || file.ContentType is null)
                    return false;

                if (!await CheckMIME(file.ContentType))
                    return false;

                using var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var virusCheckTask = _virusCheck.GetResultScan(file, cancellationToken);
                var timeoutTask = Task.Delay(TASK_AWAITING);
                var completedTask = await Task.WhenAny(virusCheckTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    cts.Cancel();
                    _logger.LogCritical("Virus check task was cancelled", nameof(CheckFile));
                    return false;
                }

                if (!await virusCheckTask)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(CheckFile));
                return false;
            }
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

        public string GetFileCategory(string contentType)
        {
            switch (contentType.Split('/')[0])
            {
                case "application":
                    return "application";
                case "audio":
                    return "audio";
                case "font":
                    return "font";
                case "image":
                    return "image";
                case "message":
                    return "message";
                case "model":
                    return "model";
                case "multipart":
                    return "multipart";
                case "text":
                    return "text";
                case "video":
                    return "video";
                default:
                    throw new ArgumentException("Invalid MIME type");
            }
        }

        public async Task CreateFile(int userID, string uniqueFileName, string mime, string mimeCategory, string fileType)
        {
            await _fileRepository.Add(new FileModel
            {
                user_id = userID,
                file_name = uniqueFileName,
                file_mime = mime,
                file_mime_category = mimeCategory,
                operation_date = DateTime.UtcNow,
                type = fileType,
            });
        }
    }
}
