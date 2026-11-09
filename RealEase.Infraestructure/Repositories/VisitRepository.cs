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
    public class VisitRepository : BaseRepository<Visit, long>, IVisitRepository
    {
        public VisitRepository(RealEaseDbContext context) : base(context) { }

        public override async Task<IEnumerable<Visit>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<Visit?> GetByIdAsync(long id)
        {
            return await _dbSet.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Visit> CreateAsync(Visit visit)
        {
            await _dbSet.AddAsync(visit);
            await _context.SaveChangesAsync();
            return visit;
        }

        public async Task<Visit> UpdateAsync(Visit visit)
        {
            var existingVisit = await GetByIdAsync(visit.Id);
            _context.Entry(existingVisit!).CurrentValues.SetValues(visit);
            await _context.SaveChangesAsync();
            return existingVisit!;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var visit = await GetByIdAsync(id);
            _dbSet.Remove(visit!);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Visit>> GetFilteredVisitsAsync(DateTime? visitdate, long? propertyid, Guid? clientid, string? status)
        {
            var query = _dbSet.AsQueryable();

            if (visitdate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(visitdate.Value.Date, DateTimeKind.Utc);
                var endUtc = startUtc.AddDays(1);
                query = query.Where(v => v.VisitDate >= startUtc && v.VisitDate < endUtc);
            }

            if (propertyid.HasValue)
                query = query.Where(u => u.PropertyId == propertyid.Value);

            if (clientid.HasValue)
                query = query.Where(u => u.UserId == clientid.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(u => u.Status == status);

            return await query.ToListAsync();
        }
    }
}
