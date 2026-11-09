using RealEase.Domain.Entities;
using RealEase.Application.Contracts.Persistence;
using System;
using System.Threading.Tasks;

namespace RealEase.Application.Contracts.Persistence
{
    /// <summary>
    /// Interfaz de repositorio específica para la gestión de entidades de Usuario.
    /// Hereda las operaciones CRUD genéricas de IBaseRepository.
    /// </summary>
    public interface IUserRepository : IBaseRepository<User>
    {
        /// <summary>
        /// Obtiene un usuario mediante su ID único (Guid).
        /// </summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene un usuario mediante su dirección de correo electrónico.
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Obtiene un usuario mediante su nombre de usuario (se mapea a Email como username).
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Verifica si un correo electrónico ya está registrado en el sistema.
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Verifica si un nombre de usuario ya está en uso.
        /// </summary>
        Task<bool> ExistsByUsernameAsync(string username);
    }
}
