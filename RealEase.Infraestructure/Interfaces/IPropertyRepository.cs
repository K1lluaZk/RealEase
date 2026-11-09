using RealEase.Application.Contracts.Persistence;
using RealEase.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Interfaces
{
    public interface IPropertyRepository : IBaseRepository<Property>
    {
        Task<Property?> GetByIdAsync(long id);
        Task<Property> CreateAsync(Property property);
        Task<Property> UpdateAsync(Property property);
        Task<bool> DeleteAsync(long id);
        Task<List<Property>> GetFilteredPropertiesAsync(string? address, decimal? minPrice, decimal? maxPrice, string? type, string? status);
    }
}
