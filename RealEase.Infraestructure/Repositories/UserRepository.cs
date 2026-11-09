using Microsoft.EntityFrameworkCore;
using RealEase.Application.Contracts.Persistence;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Core;
using RealEase.Persistence.Context;
using System;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User, Guid>, IUserRepository
    {
        public UserRepository(RealEaseDbContext db) : base(db)
        {
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            // El Email actúa como username único en este sistema
            return await _dbSet.AnyAsync(u => u.Email == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            // Busca por email ya que es el campo de identificación único
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == username);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
