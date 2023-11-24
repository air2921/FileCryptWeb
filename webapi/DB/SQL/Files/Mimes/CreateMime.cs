using Microsoft.EntityFrameworkCore;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL.Files.Mimes
{
    public class CreateMime : ICreate<FileMimeModel>, IInsertBase<FileMimeModel>
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<CreateMime> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileManager _fileManager;

        public CreateMime(FileCryptDbContext dbContext, ILogger<CreateMime> logger, IWebHostEnvironment webHostEnvironment, IFileManager fileManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _fileManager = fileManager;
        }

        public async Task Create(FileMimeModel mimeModel)
        {
            await _dbContext.AddAsync(mimeModel);
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
                    new FileMimeModel { mime_name = "application/javascript" },
                    new FileMimeModel { mime_name = "application/x-javascript" },
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
                var allMimesFromCSV = new HashSet<string>();

                foreach (var dataFile in dataFiles)
                {
                    try
                    {
                        allMimesFromCSV.UnionWith(_fileManager.GetMimesFromCsvFile(dataFile));
                    }
                    catch (IOException ex)
                    {
                        _logger.LogCritical(ex.ToString(), nameof(DBInsertBase));
                    }
                }

                var existingMimes = await _dbContext.Mimes.Select(m => m.mime_name).ToListAsync();

                var hashSetExistingMimes = existingMimes.ToHashSet();

                var mimes = allMimesFromCSV.Where(mime => !hashSetExistingMimes.Contains(mime)).ToHashSet();

                allMimes.UnionWith(mimes);

                foreach (var newMime in allMimes)
                {
                    var mimeType = new FileMimeModel { mime_name = newMime };
                    await _dbContext.AddAsync(mimeType);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
