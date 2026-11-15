using Microsoft.EntityFrameworkCore;
using RealEase.Application.Dtos.AgentInquiry;
using RealEase.Domain.Entities;
using RealEase.Domain.Models;
using RealEase.Persistence.Context;

namespace RealEase.Application.Services;

public sealed class AgentInquiryService(RealEaseDbContext dbContext)
{
    public async Task<AgentInquiryResponseDto?> CreateAsync(
        AgentInquiryDto dto,
        CancellationToken cancellationToken = default)
    {
        Property? property = null;
        if (dto.Intent == "rent")
        {
            if (!dto.PropertyId.HasValue)
                throw new ArgumentException("A rental inquiry must include a property.", nameof(dto));

            property = await dbContext.Properties.AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == dto.PropertyId, cancellationToken);
            if (property is null || !string.Equals(property.ListingType, "rent", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The selected rental property is not available.", nameof(dto));
        }
        else if (dto.PropertyId.HasValue)
        {
            throw new ArgumentException("A seller inquiry cannot include a rental property.", nameof(dto));
        }

        var users = await dbContext.Users.AsNoTracking()
            .Where(user => user.IsActive)
            .ToListAsync(cancellationToken);

        var agent = property?.OwnerId is Guid ownerId
            ? users.FirstOrDefault(user =>
                user.Id == ownerId && UserRoles.Normalize(user.Role) == UserRoles.Agent)
            : null;

        var location = property?.City;
        if (agent is null)
        {
            location = string.IsNullOrWhiteSpace(location) ? dto.Location.Trim() : location.Trim();
            agent = users.FirstOrDefault(user =>
                UserRoles.Normalize(user.Role) == UserRoles.Agent &&
                (user.ServiceArea?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Any(area => string.Equals(area, location, StringComparison.OrdinalIgnoreCase)) ?? false));
        }

        if (agent is null)
            return null;

        var inquiry = new AgentInquiry
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Location = location ?? dto.Location.Trim(),
            Intent = dto.Intent,
            Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
            PropertyId = property?.Id,
            AgentId = agent.Id,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.AgentInquiries.Add(inquiry);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AgentInquiryResponseDto
        {
            AgentName = $"{agent.FirstName} {agent.LastName}".Trim(),
            AgentEmail = agent.Email,
            AgentPhoneNumber = agent.PhoneNumber
        };
    }
}
