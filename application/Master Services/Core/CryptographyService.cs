using application.Abstractions.Endpoints.Core;
using application.Abstractions.Inner;
using application.DTO.Outer;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace application.Master_Services.Core
{
    public class CryptographyService(
        IFileHelper fileHelper,
        ICryptographyHelper cryptographyHelper,
        IRedisCache redisCache,
        ILogger<CryptographyService> logger) : ICryptographyService
    {
        private const string DISK_NAME = "C:";
        private const string FOLDER = "FileCryptWeb";
        private readonly string FullPath = Path.Join(DISK_NAME, FOLDER);

        public async Task<Response> Cypher(CypherFileDTO dto)
        {
            if (!(dto.Operation == "encrypt" || dto.Operation == "decrypt"))
                return new Response { Status = 404, Message = Message.INVALID_ROUTE };

            var filename = Guid.NewGuid().ToString() + "_" + dto.FileName;
            var path = Path.Join(FullPath, filename);

            try
            {
                if (!Directory.Exists(FullPath))
                    Directory.CreateDirectory(FullPath);

                if (!fileHelper.IsValidFile(dto.Content, dto.ContentType)/* || !await fileHelper.IsAllowedMIME(dto.ContentType)*/)
                    return new Response { Status = 422, Message = Message.INVALID_FORMAT };

                byte[]? key = await cryptographyHelper.GetKey(dto.UserId, dto.KeyId, dto.StorageId, dto.Code);
                if (key is null)
                    return new Response { Status = 400, Message = "Key not found, ot can't be formatted" };

                await fileHelper.UploadFile(path, dto.Content);
                await cryptographyHelper.CypherFile(path, dto.Operation, key);
                await fileHelper.CreateFile(dto.UserId, filename, dto.ContentType,
                    fileHelper.GetFileCategory(dto.ContentType), SetEncryptFlag(dto.Operation));

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{dto.UserId}");

                return new Response
                {
                    Status = 201,
                    ObjectData = new FileStream(path, FileMode.Open, FileAccess.Read,
                            FileShare.None, 4096, FileOptions.DeleteOnClose)
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (ArgumentException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    File.Delete(path);
                    File.Delete($"{path}.tmp");

                    return new Response { Status = 422, Message = ex.Message };
                }
                catch (Exception exc)
                {
                    logger.LogError(exc.ToString());
                    return new Response { Status = 422, Message = ex.Message };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new Response { Status = 500, Message = Message.ERROR };
            }
        }

        private bool SetEncryptFlag(string operation)
        {
            if (operation == "encrypt")
                return true;

            return false;
        }
    }
}
