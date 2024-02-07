using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces;

namespace webapi.DB
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ILogger<Repository<T>> _logger;
        private readonly FileCryptDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ILogger<Repository<T>> logger, FileCryptDbContext context)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll(Func<IQueryable<T>, IQueryable<T>> queryModifier = null)
        {
            IQueryable<T> query = _dbSet;

            if (queryModifier is not null)
                query = queryModifier(query);

            return await query.ToListAsync();
        }

        public async Task<T> GetByFilter(Func<IQueryable<T>, IQueryable<T>>? queryModifier)
        {
            IQueryable<T> query = _dbSet;

            if (queryModifier is not null)
                query = queryModifier(query);

            return await query.FirstOrDefaultAsync() ??
                throw new EntityException();
        }

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id) ??
                throw new EntityException();
        }

        public async Task<int> Add(T entity, Func<T, int>? GetId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (GetId is not null)
                    return GetId(entity);

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(_context), nameof(_dbSet));
                await transaction.RollbackAsync();
                throw new EntityException();
            }
        }

        public async Task AddRange(IEnumerable<T> entities)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _dbSet.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogCritical(ex.ToString(), nameof(_context), nameof(_dbSet));
                throw new EntityException();
            }
        }

        public async Task Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity is not null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogCritical(ex.ToString(), nameof(_context), nameof(_dbSet));
            }
        }

        public async Task DeleteMany(IEnumerable<int> identifiers)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var id in identifiers)
                {
                    var entity = await _dbSet.FindAsync(id);
                    if (entity != null)
                    {
                        _dbSet.Remove(entity);
                    }
                }
                await transaction.CommitAsync();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogCritical(ex.ToString(), nameof(_context), nameof(_dbSet));
            }
        }

        public async Task Update(T entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogCritical(ex.ToString(), nameof(_context), nameof(_dbSet));
                throw new EntityException();
            }
        }
    }
}
