using RealEase.Application.Contracts.Persistence;
using RealEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Interfaces
{
    public interface IContractRepository : IBaseRepository<Contract>
    {
        Task<Contract?> GetByIdAsync(long id);
        Task<Contract> CreateAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(long id);
        Task<List<Contract>> GetFilteredContractsAsync(DateTime? startdate, long? propertyid, string? status);
    }
}
