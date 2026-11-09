using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEase.Infrastructure.Core;
using RealEase.Application.Dtos.Visit;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Repositories;

namespace RealEase.Application.Services
{
    public class VisitService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly VisitRepository _visitRepository;

        public VisitService(UnitOfWork unitOfWork, VisitRepository visitRepository)
        {
            _unitOfWork = unitOfWork;
            _visitRepository = visitRepository;
        }

        public async Task<List<VisitDto>> GetAllVisitsAsync()
        {
            var visits = await _visitRepository.GetAllAsync();
            var visitDtos = new List<VisitDto>();

            foreach (var visit in visits)
            {
                visitDtos.Add(new VisitDto
                {
                    Id = visit.Id,
                    PropertyId = visit.PropertyId,
                    UserId = visit.UserId, // Cambiado internamente a Guid
                    VisitDate = visit.VisitDate,
                    Status = visit.Status,
                    Notes = visit.Notes,
                    CreatedAt = visit.CreatedAt,
                    UpdatedAt = visit.UpdatedAt
                });
            }

            return visitDtos;
        }

        public async Task<VisitDto?> GetVisitByIdAsync(long id)
        {
            var visit = await _visitRepository.GetByIdAsync(id);
            if (visit == null) return null;

            return new VisitDto
            {
                Id = visit.Id,
                PropertyId = visit.PropertyId,
                UserId = visit.UserId,
                VisitDate = visit.VisitDate,
                Status = visit.Status,
                Notes = visit.Notes,
                CreatedAt = visit.CreatedAt,
                UpdatedAt = visit.UpdatedAt
            };
        }

        public async Task<long> AddVisitAsync(VisitDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var visit = new Visit
                {
                    PropertyId = dto.PropertyId,
                    UserId = dto.UserId, // Vinculación correcta con el Guid del usuario
                    VisitDate = dto.VisitDate != default ? NormalizeUtc(dto.VisitDate) : DateTime.UtcNow,
                    Status = dto.Status ?? "Pending",
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _visitRepository.AddAsync(visit);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return visit.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return 0;
            }
        }

        public async Task<bool> UpdateVisitAsync(VisitDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var visit = await _visitRepository.GetByIdAsync(dto.Id);
                if (visit == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                visit.PropertyId = dto.PropertyId;
                visit.UserId = dto.UserId;
                visit.VisitDate = NormalizeUtc(dto.VisitDate);
                visit.Status = dto.Status;
                visit.Notes = dto.Notes;
                visit.UpdatedAt = DateTime.UtcNow;

                // Corregido: Se utiliza el método síncrono del repositorio base
                _visitRepository.Update(visit);

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

        public async Task<bool> DeleteVisitAsync(long id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var visit = await _visitRepository.GetByIdAsync(id);
                if (visit == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Corregido: Remueve pasando la entidad trackeada directamente
                _visitRepository.Remove(visit);

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

        public async Task<List<VisitDto>> GetFilteredVisitsAsync(DateTime? visitdate, long? propertyid, Guid? clientid, string? status)
        {
            // Nota: Cambiamos la firma del parámetro clientid a Guid? para coincidir con la BD
            var utcVisitDate = visitdate.HasValue ? NormalizeUtc(visitdate.Value) : (DateTime?)null;
            var visits = await _visitRepository.GetFilteredVisitsAsync(utcVisitDate, propertyid, clientid, status);

            return visits.Select(u => new VisitDto
            {
                Id = u.Id,
                PropertyId = u.PropertyId,
                UserId = u.UserId,
                VisitDate = u.VisitDate,
                Status = u.Status,
                Notes = u.Notes,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
