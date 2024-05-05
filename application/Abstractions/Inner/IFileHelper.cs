namespace application.Abstractions.Inner
{
    public interface IFileHelper
    {
        public bool IsValidFile(Stream stream, string contentType);
        public Task<bool> IsAllowedMIME(string mime);
        public Task UploadFile(string filePath, Stream file);
        public Task CreateFile(int userID, string uniqueFileName, string mime, string mimeCategory, bool encrypt);
        string GetFileCategory(string contentType);
    }
}
