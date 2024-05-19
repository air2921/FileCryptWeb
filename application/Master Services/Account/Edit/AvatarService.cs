using application.Abstractions.Endpoints.Account;
using application.Abstractions.TP_Services;
using application.DTO.Outer;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Master_Services.Account.Edit
{
    public class AvatarService(
        IS3Manager s3Manager,
        IDatabaseTransaction dbTransaction,
        IRepository<UserModel> userRepository) : IAvatarService
    {
        public async Task<Response> Delete(int userId)
        {
            var transaction = await dbTransaction.BeginAsync();

            try
            {
                var user = await userRepository.GetById(userId);
                if (user is null || user.avatarId is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                var avatar = user.avatarId;
                user.avatarId = null;
                user.avatar_content_type = null;
                user.avatar_name = null;
                await userRepository.Update(user);
                await s3Manager.Delete(avatar);

                await dbTransaction.CommitAsync(transaction);

                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                await dbTransaction.RollbackAsync(transaction);
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (S3ClientException ex)
            {
                await dbTransaction.RollbackAsync(transaction);
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> Change(Stream stream, string name, string contentType, int userId, string avatarId)
        {
            var transaction = await dbTransaction.BeginAsync();

            try
            {
                var user = await userRepository.GetById(userId);
                if (user is null || user.avatarId is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                user.avatarId = avatarId;
                user.avatar_name = name;
                user.avatar_content_type = contentType;
                await userRepository.Update(user);
                await s3Manager.Upload(stream, user.avatarId);

                await dbTransaction.CommitAsync(transaction);

                return new Response { Status = 204, Message = Message.UPDATED };
            }
            catch (EntityException ex)
            {
                await dbTransaction.RollbackAsync(transaction);
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (S3ClientException ex)
            {
                await dbTransaction.RollbackAsync(transaction);
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> Download(int userId)
        {
            try
            {
                var user = await userRepository.GetById(userId);
                if (user is null || user.avatarId is null || user.avatar_content_type is null || user.avatar_name is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return new Response
                {
                    Status = 200,
                    ObjectData = new AvatarDTO
                    {
                        AvatarContent = await s3Manager.Download(user.avatarId),
                        ContentType = user.avatar_content_type,
                        Name = user.avatar_name
                    }
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (S3ClientException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
