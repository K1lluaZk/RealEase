using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEase.Application.Dtos.Payment;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Core;
using RealEase.Infrastructure.Repositories;

namespace RealEase.Application.Services
{
    public class PaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly PaymentRepository _paymentRepository;

        public PaymentService(UnitOfWork unitOfWork, PaymentRepository paymentRepository)
        {
            _unitOfWork = unitOfWork;
            _paymentRepository = paymentRepository;
        }

        public async Task<List<PaymentDto>> GetAllPaymentsAsync(string? filter = null)
        {
            var payments = await _paymentRepository.GetAllAsync();

            var paymentDtos = new List<PaymentDto>();
            foreach (var payment in payments)
            {
                paymentDtos.Add(new PaymentDto
                {
                    Id = payment.Id,
                    ContractId = payment.ContractId,
                    TenantId = payment.TenantId,
                    PaymentDate = payment.PaymentDate,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status ?? string.Empty
                });
            }

            return paymentDtos;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(long id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null) return null;

            return new PaymentDto
            {
                Id = payment.Id,
                ContractId = payment.ContractId,
                TenantId = payment.TenantId,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status ?? string.Empty
            };
        }

        public async Task<long> AddPaymentAsync(PaymentDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = new Payment
                {
                    ContractId = dto.ContractId,
                    TenantId = dto.TenantId,
                    PaymentDate = dto.PaymentDate == default
                        ? DateTime.UtcNow
                        : DateTime.SpecifyKind(dto.PaymentDate, DateTimeKind.Utc),
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    Status = dto.Status
                };

                await _paymentRepository.AddAsync(payment);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return payment.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return 0;
            }
        }

        public async Task<bool> UpdatePaymentAsync(PaymentDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetByIdAsync(dto.Id);
                if (payment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                payment.ContractId = dto.ContractId;
                payment.TenantId = dto.TenantId;
                payment.PaymentDate = DateTime.SpecifyKind(dto.PaymentDate, DateTimeKind.Utc);
                payment.Amount = dto.Amount;
                payment.PaymentMethod = dto.PaymentMethod;
                payment.Status = dto.Status;

                _paymentRepository.Update(payment);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> DeletePaymentAsync(long id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var payment = await _paymentRepository.GetByIdAsync(id);
                if (payment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                _paymentRepository.Remove(payment);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<List<PaymentDto>> GetFilteredPaymentsAsync(DateTime? paymentdate, long? contractid, Guid? clientid)
        {
            var payments = await _paymentRepository.GetFilteredPaymentsAsync(paymentdate, contractid, clientid);

            return payments.Select(u => new PaymentDto
            {
                Id = u.Id,
                ContractId = u.ContractId,
                TenantId = u.TenantId,
                PaymentDate = u.PaymentDate,
                Amount = u.Amount,
                PaymentMethod = u.PaymentMethod,
                Status = u.Status ?? string.Empty
            }).ToList();
        }
    }
}
