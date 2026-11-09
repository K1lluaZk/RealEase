using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEase.Infrastructure.Core;         // <-- Corregido (Infraestructure con 'es')
using RealEase.Infrastructure.Repositories;
using RealEase.Application.Dtos.User;
using RealEase.Domain.Entities;

namespace RealEase.Application.Services
{
    public class UserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserRepository _userRepository;

        public UserService(UnitOfWork unitOfWork, UserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(new UserDto
                {
                    Id = user.Id, // Ahora es Guid
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
            }

            return userDtos;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id) // ¡Cambiado a Guid!
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<Guid> AddUserAsync(UserDto dto) // Retorna Guid
        {
            // public.users.id referencia auth.users.id; Auth es quien crea el UUID.
            if (dto.Id == Guid.Empty)
                return Guid.Empty;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = new User
                {
                    Id = dto.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = dto.Role,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return user.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Guid.Empty;
            }
        }

        public async Task<bool> UpdateUserAsync(UserDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _userRepository.GetByIdAsync(dto.Id);
                if (user == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.PhoneNumber = dto.PhoneNumber;
                user.Role = dto.Role;
                user.IsActive = dto.IsActive;

                // Corregido: Llamada al método síncrono de IRepository
                _userRepository.Update(user);

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

        public async Task<bool> DeleteUserAsync(Guid id) // ¡Cambiado a Guid!
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Corregido: Pasa la entidad completa al método genérico síncrono Remove
                _userRepository.Remove(user);

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

        public async Task<List<UserDto>> GetFilteredUsersAsync(string? name, string? role, bool? isActive = null)
        {
            var users = await _userRepository.GetAllAsync();
            var query = users.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return query.Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }
    }
}
