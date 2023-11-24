using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL.API
{
    public class CreateAPI : ICreate<ApiModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public CreateAPI(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Create(ApiModel apiModel)
        {
            var newApiModel = new ApiModel
            {
                user_id = apiModel.user_id,
                api_key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString(),
                remote_ip = apiModel.remote_ip,
                is_tracking_ip = true,
                is_allowed_requesting = true,
                is_allowed_unknown_ip = false,
            };

            await _dbContext.AddAsync(newApiModel);
            await _dbContext.SaveChangesAsync();
        }
    }
}
