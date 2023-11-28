﻿using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Files : ICreate<FileModel>, IRead<FileModel>, IDelete<FileModel>, IDeleteByName<FileModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Files(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(FileModel fileModel)
        {
            await _dbContext.AddAsync(fileModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<FileModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Files.FirstOrDefaultAsync(f => f.file_id == id) ??
                throw new FileException("");
        }

        public async Task<IEnumerable<FileModel>> ReadAll()
        {
            return await _dbContext.Files.ToListAsync();
        }

        public async Task DeleteById(int id)
        {
            var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.file_id == id) ??
                throw new FileException("");

            _dbContext.Remove(file);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string fileName)
        {
            var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.file_name == fileName) ??
                throw new FileException("");

            _dbContext.Remove(file);
            await _dbContext.SaveChangesAsync();
        }
    }
}
