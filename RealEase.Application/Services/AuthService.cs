using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase.Gotrue;
using RealEase.Application.Dtos.Supabase;
using RealEase.Domain.Models.Supabase;

namespace RealEase.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
        Task<AuthResponse> LoginAsync(LoginUserRequest request);
        Task<ProfileModel?> GetProfileAsync(string userId);
        Task<ProfileModel> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<bool> AssignRoleAsync(string userId, UserRole newRole);
    }

    public class AuthService : IAuthService
    {
        private readonly Supabase.Client _supabase;

        public AuthService(Supabase.Client supabase)
        {
            _supabase = supabase ?? throw new ArgumentNullException(nameof(supabase));
        }

        /// <summary>
        /// Registers a new user with Supabase Auth and triggers automatic profile creation.
        /// Sends custom metadata (role, full_name, phone) to be processed by PostgreSQL trigger.
        /// </summary>
        public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required.");

            var roleStr = request.Role.ToString().ToLowerInvariant();
            var (firstName, lastName) = SplitName(request.FullName ?? request.Email.Split('@')[0]);

            var signUpOptions = new SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "role", roleStr },
                    { "first_name", firstName },
                    { "last_name", lastName },
                    { "phone_number", request.Phone ?? string.Empty }
                }
            };

            var session = await _supabase.Auth.SignUp(request.Email, request.Password, signUpOptions);
            if (session?.User?.Id == null)
            {
                throw new InvalidOperationException("Failed to register user in Supabase Auth.");
            }

            // Fetch created profile from public.profiles table (created by trigger)
            ProfileModel? profile = null;
            try
            {
                var profileResponse = await _supabase
                    .From<ProfileModel>()
                    .Where(p => p.Id == session.User.Id)
                    .Single();

                profile = profileResponse;
            }
            catch
            {
                // Fallback if trigger replication is delayed
            }

            return new AuthResponse
            {
                AccessToken = session.AccessToken ?? string.Empty,
                RefreshToken = session.RefreshToken ?? string.Empty,
                UserId = session.User.Id,
                Email = session.User.Email ?? request.Email,
                FullName = profile is null ? request.FullName : JoinName(profile.FirstName, profile.LastName),
                Role = profile?.Role ?? roleStr
            };
        }

        /// <summary>
        /// Signs in an existing user and returns current auth tokens and profile.
        /// </summary>
        public async Task<AuthResponse> LoginAsync(LoginUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required.");

            var session = await _supabase.Auth.SignIn(request.Email, request.Password);
            if (session?.User?.Id == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var profile = await GetProfileAsync(session.User.Id);

            return new AuthResponse
            {
                AccessToken = session.AccessToken ?? string.Empty,
                RefreshToken = session.RefreshToken ?? string.Empty,
                UserId = session.User.Id,
                Email = session.User.Email ?? request.Email,
                FullName = profile is null ? null : JoinName(profile.FirstName, profile.LastName),
                Role = profile?.Role ?? "guest"
            };
        }

        /// <summary>
        /// Retrieves user profile details by User ID.
        /// </summary>
        public async Task<ProfileModel?> GetProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var response = await _supabase
                .From<ProfileModel>()
                .Where(p => p.Id == userId)
                .Get();

            return response.Models.Count > 0 ? response.Models[0] : null;
        }

        /// <summary>
        /// Updates profile information (full name, avatar, phone).
        /// </summary>
        public async Task<ProfileModel> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            var profile = await GetProfileAsync(userId)
                ?? throw new KeyNotFoundException($"Profile with ID {userId} not found.");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                profile.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                profile.LastName = request.LastName;

            if (request.PhoneNumber is not null)
                profile.PhoneNumber = request.PhoneNumber;

            var response = await _supabase
                .From<ProfileModel>()
                .Update(profile);

            return response.Models[0];
        }

        /// <summary>
        /// Assigns a new role (e.g. upgrading Guest to Host or Administrator).
        /// </summary>
        public async Task<bool> AssignRoleAsync(string userId, UserRole newRole)
        {
            var profile = await GetProfileAsync(userId)
                ?? throw new KeyNotFoundException($"Profile with ID {userId} not found.");

            profile.Role = newRole.ToString().ToLowerInvariant();
            var response = await _supabase
                .From<ProfileModel>()
                .Update(profile);

            return response.Models.Count > 0;
        }

        private static (string FirstName, string LastName) SplitName(string fullName)
        {
            var parts = fullName.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => (string.Empty, string.Empty),
                1 => (parts[0], string.Empty),
                _ => (parts[0], parts[1])
            };
        }

        private static string JoinName(string firstName, string lastName) =>
            string.Join(' ', new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
