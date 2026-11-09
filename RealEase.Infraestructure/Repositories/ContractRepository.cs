using Microsoft.EntityFrameworkCore;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Core;
using RealEase.Infrastructure.Exceptions;
using RealEase.Infrastructure.Interfaces;
using RealEase.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Repositories
{
    public class ContractRepository : BaseRepository<Contract, long>, IContractRepository
    {
        public ContractRepository(RealEaseDbContext context) : base(context) { }

        public override async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<Contract?> GetByIdAsync(long id)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Id == id)
                   ?? throw new ContractException("Contrato no encontrado.");
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            await _dbSet.AddAsync(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            var existingContract = await GetByIdAsync(contract.Id);
            _context.Entry(existingContract!).CurrentValues.SetValues(contract);
            await _context.SaveChangesAsync();
            return existingContract!;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var contract = await GetByIdAsync(id);
            if (contract == null) return false;

            _dbSet.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Contract>> GetFilteredContractsAsync(DateTime? startdate, long? propertyid, string? status)
        {
            var query = _context.Contracts.AsQueryable();

            if (startdate.HasValue)
                query = query.Where(u => u.StartDate.Date == startdate.Value.Date);

            if (propertyid.HasValue)
                query = query.Where(u => u.PropertyId == propertyid.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(u => u.Status == status);

            return await query.ToListAsync();
        }
    }
}
