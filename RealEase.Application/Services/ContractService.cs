using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEase.Infrastructure.Core;
using RealEase.Application.Dtos.Contract;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Repositories;

namespace RealEase.Application.Services
{
    public class ContractService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ContractRepository _contractRepository;

        public ContractService(UnitOfWork unitOfWork, ContractRepository contractRepository)
        {
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
        }

        public async Task<List<ContractDto>> GetAllContractsAsync()
        {
            var contracts = await _contractRepository.GetAllAsync();

            var contractDtos = new List<ContractDto>();
            foreach (var contract in contracts)
            {
                contractDtos.Add(new ContractDto
                {
                    Id = contract.Id,
                    ClientId = contract.ClientId,
                    AgentId = contract.AgentId,
                    PropertyId = contract.PropertyId,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    TotalAmount = contract.TotalAmount,
                    Status = contract.Status ?? string.Empty
                });
            }

            return contractDtos;
        }

        public async Task<ContractDto?> GetContractByIdAsync(long id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null) return null;

            return new ContractDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                AgentId = contract.AgentId,
                PropertyId = contract.PropertyId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                TotalAmount = contract.TotalAmount,
                Status = contract.Status ?? string.Empty
            };
        }

        public async Task<long> AddContractAsync(ContractDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var contract = new Contract
                {
                    ClientId = dto.ClientId,
                    AgentId = dto.AgentId,
                    PropertyId = dto.PropertyId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    TotalAmount = dto.TotalAmount,
                    Status = dto.Status
                };

                await _contractRepository.AddAsync(contract);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return contract.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return 0;
            }
        }

        public async Task<bool> UpdateContractAsync(ContractDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var contract = await _contractRepository.GetByIdAsync(dto.Id);
                if (contract == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                contract.ClientId = dto.ClientId;
                contract.AgentId = dto.AgentId;
                contract.PropertyId = dto.PropertyId;
                contract.StartDate = dto.StartDate;
                contract.EndDate = dto.EndDate;
                contract.TotalAmount = dto.TotalAmount;
                contract.Status = dto.Status;

                await _contractRepository.UpdateAsync(contract);
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

        public async Task<bool> DeleteContractAsync(long id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var contract = await _contractRepository.GetByIdAsync(id);
                if (contract == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                await _contractRepository.DeleteAsync(id);
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

        public async Task<List<ContractDto>> GetFilteredContractsAsync(DateTime? startdate, long? propertyid, string? status)
        {
            var contracts = await _contractRepository.GetFilteredContractsAsync(startdate, propertyid, status);

            return contracts.Select(u => new ContractDto
            {
                Id = u.Id,
                ClientId = u.ClientId,
                AgentId = u.AgentId,
                PropertyId = u.PropertyId,
                StartDate = u.StartDate,
                EndDate = u.EndDate,
                TotalAmount = u.TotalAmount,
                Status = u.Status ?? string.Empty
            }).ToList();
        }
    }
}
