namespace webapi.Interfaces.Controllers.Services
{
    public interface IFileService
    {
        public bool CheckFileType(string type);
        public bool CheckFile(IFormFile file);
        public Task<bool> IsProhibitedMIME(string mime);
        public Task UploadFile(string filePath, IFormFile file);
        public Task CreateFile(int userID, string uniqueFileName, string mime, string mimeCategory, string fileType);
        string GetFileCategory(string contentType);
    }
}
