using RealEase.Application.Contracts.Persistence;
using RealEase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.Infrastructure.Interfaces
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<Payment?> GetByIdAsync(long id);
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(long id);
        Task<List<Payment>> GetFilteredPaymentsAsync(DateTime? paymentDate, long? contractId, Guid? clientId);
    }
}
