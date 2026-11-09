using Microsoft.EntityFrameworkCore.Storage;
using RealEase.Persistence.Context;

namespace RealEase.Infrastructure.Core
{
    public class UnitOfWork
    {
        private readonly RealEaseDbContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(RealEaseDbContext context)
        {
            _context = context;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}