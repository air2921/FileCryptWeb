﻿namespace webapi.Interfaces.Controllers
{
    public interface IFileService
    {
        public bool CheckFileType(string type);
        public Task<bool> CheckFile(IFormFile file);
        public bool CheckSize(IFormFile file);
        public Task UploadFile(string filePath, IFormFile file);
        public Task CreateFile(int userID, string uniqueFileName, string mime, string mimeCategory, string fileType);
        string GetFileCategory(string contentType);
    }
}
