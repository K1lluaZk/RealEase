using RealEase.Application.Dtos.Property;
using RealEase.Domain.Entities;
using RealEase.Infrastructure.Core;
using RealEase.Infrastructure.Repositories;

namespace RealEase.Application.Services
{
    public class PropertyService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly PropertyRepository _propertyRepository;

        public PropertyService(UnitOfWork unitOfWork, PropertyRepository propertyRepository)
        {
            _unitOfWork = unitOfWork;
            _propertyRepository = propertyRepository;
        }

        public async Task<List<PropertyDto>> GetAllPropertiesAsync()
        {
            var properties = await _propertyRepository.GetAllAsync();
            return properties.Select(ToDto).ToList();
        }

        public async Task<PropertyDto?> GetPropertyByIdAsync(long id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            return property is null ? null : ToDto(property);
        }

        public async Task<long> AddPropertyAsync(PropertyDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var property = new Property
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Address = dto.Address,
                    PricePerNight = dto.PricePerNight,
                    PropertyType = dto.PropertyType,
                    Status = dto.Status,
                    OwnerId = dto.OwnerId,
                    Images = dto.Images,
                    Bedrooms = dto.Bedrooms,
                    Bathrooms = dto.Bathrooms,
                    MaxGuests = dto.MaxGuests,
                    CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt,
                    UpdatedAt = dto.UpdatedAt
                };

                await _propertyRepository.CreateAsync(property);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return property.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return 0;
            }
        }

        public async Task<bool> UpdatePropertyAsync(PropertyDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var property = await _propertyRepository.GetByIdAsync(dto.Id);
                if (property is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                property.Title = dto.Title;
                property.Description = dto.Description;
                property.Address = dto.Address;
                property.PricePerNight = dto.PricePerNight;
                property.PropertyType = dto.PropertyType;
                property.Status = dto.Status;
                property.OwnerId = dto.OwnerId;
                property.Images = dto.Images;
                property.Bedrooms = dto.Bedrooms;
                property.Bathrooms = dto.Bathrooms;
                property.MaxGuests = dto.MaxGuests;
                property.UpdatedAt = DateTime.UtcNow;

                await _propertyRepository.UpdateAsync(property);
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

        public async Task<bool> DeletePropertyAsync(long id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var property = await _propertyRepository.GetByIdAsync(id);
                if (property is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                await _propertyRepository.DeleteAsync(id);
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

        public async Task<List<PropertyDto>> GetFilteredPropertiesAsync(
            string? address, string? type, decimal? minPrice, decimal? maxPrice, string? status)
        {
            var properties = await _propertyRepository.GetFilteredPropertiesAsync(address, minPrice, maxPrice, type, status);
            return properties.Select(ToDto).ToList();
        }

        private static PropertyDto ToDto(Property property) => new()
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Address = property.Address,
            PricePerNight = property.PricePerNight,
            PropertyType = property.PropertyType,
            Status = property.Status,
            OwnerId = property.OwnerId,
            Images = property.Images,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            MaxGuests = property.MaxGuests,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }
}
