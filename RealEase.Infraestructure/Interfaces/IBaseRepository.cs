using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RealEase.Application.Contracts.Persistence
{
    /// <summary>
    /// Contrato genérico base para todos los repositorios del sistema.
    /// Define las operaciones CRUD estándar que deben implementar todos los repositorios.
    /// </summary>
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
