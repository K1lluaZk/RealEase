using Microsoft.EntityFrameworkCore;
using RealEase.Application.Contracts.Persistence;
using RealEase.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Core
{
    /// <summary>
    /// Implementación base genérica de repositorio que implementa IBaseRepository<T>.
    /// Todas las entidades comparten estas operaciones CRUD a través de herencia.
    /// TId permite que el ID sea int o Guid según la entidad.
    /// </summary>
    public class BaseRepository<T, TId> : IBaseRepository<T> where T : class
    {
        protected readonly RealEaseDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(RealEaseDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Búsqueda por ID genérico (acepta int o Guid según la entidad).
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(TId id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
