using RealEase.Application.Contracts.Persistence;
using RealEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Interfaces
{
    public interface IVisitRepository : IBaseRepository<Visit>
    {
        Task<Visit?> GetByIdAsync(long id);
        Task<Visit> CreateAsync(Visit visit);
        Task<Visit> UpdateAsync(Visit visit);
        Task<bool> DeleteAsync(long id);
        Task<List<Visit>> GetFilteredVisitsAsync(DateTime? visitdate, long? propertyid, Guid? clientid, string? status);
    }
}
