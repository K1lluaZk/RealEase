using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Core;
using RealEase.Infrastructure.Interfaces;
using RealEase.Persistence.Context;

namespace RealEase.Infrastructure.Repositories
{
    public class PaymentRepository : BaseRepository<Payment, long>, IPaymentRepository
    {
        public PaymentRepository(RealEaseDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<Payment?> GetByIdAsync(long id)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            await _dbSet.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            var existing = await GetByIdAsync(payment.Id);
            _context.Entry(existing!).CurrentValues.SetValues(payment);
            await _context.SaveChangesAsync();
            return existing!;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var payment = await GetByIdAsync(id);
            if (payment == null) return false;
            _dbSet.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Payment>> GetFilteredPaymentsAsync(DateTime? paymentDate, long? contractId, Guid? clientId)
        {
            var query = _context.Set<Payment>().AsQueryable();

            if (paymentDate.HasValue)
                query = query.Where(p => p.PaymentDate.Date == paymentDate.Value.Date);

            if (contractId.HasValue)
                query = query.Where(p => p.ContractId == contractId.Value);

            if (clientId.HasValue)
                query = query.Where(p => p.TenantId == clientId.Value);

            return await query.ToListAsync();
        }
    }
}
