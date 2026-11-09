using Microsoft.EntityFrameworkCore;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Core;
using RealEase.Infrastructure.Exceptions;
using RealEase.Infrastructure.Interfaces;
using RealEase.Persistence.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Repositories
{
    public class PropertyRepository : BaseRepository<Property, long>, IPropertyRepository
    {
        public PropertyRepository(RealEaseDbContext context) : base(context) { }

        public override async Task<IEnumerable<Property>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<Property?> GetByIdAsync(long id)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Property> CreateAsync(Property property)
        {
            await _dbSet.AddAsync(property);
            await _context.SaveChangesAsync();
            return property;
        }

        public async Task<Property> UpdateAsync(Property property)
        {
            var existingProperty = await GetByIdAsync(property.Id);
            _context.Entry(existingProperty!).CurrentValues.SetValues(property);
            await _context.SaveChangesAsync();
            return existingProperty!;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var property = await GetByIdAsync(id);
            if (property == null) return false;

            _dbSet.Remove(property);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Property>> GetFilteredPropertiesAsync(string? address, decimal? minPrice, decimal? maxPrice, string? type, string? status)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(address))
                query = query.Where(p => p.Address.Contains(address));

            if (minPrice.HasValue)
                query = query.Where(p => p.PricePerNight >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.PricePerNight <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(u => u.PropertyType.Contains(type));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(u => u.Status != null && u.Status.Contains(status));

            return await query.ToListAsync();
        }
    }
}
