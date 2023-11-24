namespace webapi.Interfaces.Controllers
{
    public interface IFileService
    {
        public Task<bool> CheckFile(IFormFile? file);
        public bool CheckSize(IFormFile file);
        public Task UploadFile(string filePath, IFormFile file);
        public Task DeleteFile(string filePath);
        public Task CreateFile(int userID, string uniqueFileName, string mime, string fileType);
    }
}
