using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;
using webapi.Services;

namespace webapi.DB.SQL
{
    public class Mimes : ICreate<FileMimeModel>, IRead<FileMimeModel>, IDelete<FileMimeModel>, IDeleteByName<FileMimeModel>, IInsertBase<FileMimeModel>
    {
        private readonly IRedisCache _redisCache;
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<Mimes> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileManager _fileManager;

        public Mimes(IRedisCache redisCache, FileCryptDbContext dbContext, ILogger<Mimes> logger, IWebHostEnvironment webHostEnvironment, IFileManager fileManager)
        {
            _redisCache = redisCache;
            _dbContext = dbContext;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _fileManager = fileManager;
        }

        public async Task Create(FileMimeModel mimeModel)
        {
            await _dbContext.AddAsync(mimeModel);
            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DBInsertBase(FileMimeModel? mimeModel, bool? secure)
        {
            if (!secure.HasValue)
                throw new ArgumentNullException(nameof(secure));

            if (secure == true)
            {
                var baseData = new List<FileMimeModel>
                {
                    new FileMimeModel { mime_name = "image/jpeg" },
                    new FileMimeModel { mime_name = "image/png" },
                    new FileMimeModel { mime_name = "image/gif" },
                    new FileMimeModel { mime_name = "image/bmp" },
                    new FileMimeModel { mime_name = "image/webp" },
                    new FileMimeModel { mime_name = "image/svg+xml" },
                    new FileMimeModel { mime_name = "application/pdf" },
                    new FileMimeModel { mime_name = "application/msword" },
                    new FileMimeModel { mime_name = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                    new FileMimeModel { mime_name = "application/vnd.ms-powerpoint" },
                    new FileMimeModel { mime_name = "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                    new FileMimeModel { mime_name = "application/vnd.ms-excel" },
                    new FileMimeModel { mime_name = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                    new FileMimeModel { mime_name = "audio/mpeg" },
                    new FileMimeModel { mime_name = "audio/wav" },
                    new FileMimeModel { mime_name = "audio/mp3" },
                    new FileMimeModel { mime_name = "video/mp4" },
                    new FileMimeModel { mime_name = "video/mpeg" },
                    new FileMimeModel { mime_name = "video/webm" },
                    new FileMimeModel { mime_name = "video/mkv" },
                    new FileMimeModel { mime_name = "video/x-matroska" },
                    new FileMimeModel { mime_name = "application/zip" },
                    new FileMimeModel { mime_name = "application/x-rar-compressed" },
                    new FileMimeModel { mime_name = "application/x-tar" },
                    new FileMimeModel { mime_name = "application/x-7z-compressed" },
                    new FileMimeModel { mime_name = "text/plain" },
                    new FileMimeModel { mime_name = "text/html" },
                    new FileMimeModel { mime_name = "text/css" },
                    new FileMimeModel { mime_name = "text/xml" },
                    new FileMimeModel { mime_name = "application/json" },
                    new FileMimeModel { mime_name = "application/rtf" },
                    new FileMimeModel { mime_name = "text/richtext" },
                    new FileMimeModel { mime_name = "font/woff" },
                    new FileMimeModel { mime_name = "font/woff2" },
                    new FileMimeModel { mime_name = "font/otf" },
                    new FileMimeModel { mime_name = "font/ttf" },
                };

                await _dbContext.AddRangeAsync(baseData);
            }
            else
            {
                var basePath = Path.Combine(_webHostEnvironment.ContentRootPath, "..", "data");
                string[] dataFiles = Directory.GetFiles(basePath);

                var allMimes = new HashSet<string>();

                foreach (var dataFile in dataFiles)
                {
                    allMimes.UnionWith(_fileManager.GetMimesFromCsvFile(dataFile));
                }

                var existingMimes = (await _dbContext.Mimes.Select(m => m.mime_name).ToListAsync())
                    .Where(mime => mime != null)
                    .Select(mime => mime!)
                    .ToHashSet();

                allMimes.UnionWith(existingMimes);

                var mimeModels = new List<FileMimeModel>();
                foreach (var newMime in allMimes)
                {
                    mimeModels.Add(new FileMimeModel { mime_name = newMime });
                }

                await _dbContext.AddRangeAsync(mimeModels);
            }

            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<FileMimeModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Mimes.FirstOrDefaultAsync(m => m.mime_id == id) ??
                throw new MimeException(ExceptionMimeMessages.MimeNotFound);
        }

        public async Task<IEnumerable<FileMimeModel>> ReadAll(int? user_id, int skip, int count)
        {
            return await _dbContext.Mimes
                .Skip(skip)
                .Take(count)
                .ToListAsync() ??
                throw new MimeException(ExceptionMimeMessages.MimesNotFound);
        }

        public async Task DeleteById(int id, int? user_id)
        {
            var mime = await _dbContext.Mimes.FirstOrDefaultAsync(m => m.mime_id == id) ??
                throw new MimeException(ExceptionMimeMessages.MimeNotFound);

            _dbContext.Mimes.Remove(mime);
            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string mime_name, int? user_id)
        {
            var mime = await _dbContext.Mimes.FirstOrDefaultAsync(m => m.mime_name == mime_name) ??
                throw new MimeException(ExceptionMimeMessages.MimeNotFound);

            _dbContext.Mimes.Remove(mime);
            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
            await _dbContext.SaveChangesAsync();
        }
    }
}
