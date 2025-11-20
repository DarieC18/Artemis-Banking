using ArtemisBanking.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<Entity> : IGenericRepository<Entity>
        where Entity : class
    {
        private readonly ArtemisBankingDbContext _context;
        private readonly DbSet<Entity> _dbSet;

        public GenericRepository(ArtemisBankingDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Entity>();
        }

        public async Task<Entity?> AddAsync(Entity entity)
        {
            var entry = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<List<Entity>?> AddRangeAsync(List<Entity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity is null)
                return;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Entity>> GetAllList()
        {
            return await _dbSet.ToListAsync();
        }

        public IQueryable<Entity> GetAllQuery()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<Entity?> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<Entity>> GetAllListWithInclude(List<string> properties)
        {
            IQueryable<Entity> query = _dbSet;

            foreach (var prop in properties)
            {
                query = query.Include(prop);
            }

            return await query.ToListAsync();
        }

        public IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            IQueryable<Entity> query = _dbSet;

            foreach (var prop in properties)
            {
                query = query.Include(prop);
            }

            return query;
        }
    }
}
