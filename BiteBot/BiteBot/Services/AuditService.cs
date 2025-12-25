using System.Text.Json;
using BiteBot.Data;
using BiteBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BiteBot.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogCreateAsync(Restaurant restaurant, string username, ulong userId)
    {
        _logger.LogInformation("Logging create action for restaurant {RestaurantName} by {Username}", 
            restaurant.Name, username);

        var auditLog = new RestaurantAuditLog
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurant.Id,
            Action = AuditAction.Create,
            Timestamp = DateTime.UtcNow,
            Username = username,
            UserId = userId,
            ChangeDetails = JsonSerializer.Serialize(new
            {
                restaurant.Id,
                restaurant.Name,
                City = restaurant.City.ToString(),
                restaurant.Url
            }),
            ChangeDescription = $"Created restaurant '{restaurant.Name}' in {restaurant.City}" +
                                (string.IsNullOrWhiteSpace(restaurant.Url) ? "" : $" with URL: {restaurant.Url}")
        };

        await _context.RestaurantAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully logged create action for restaurant {RestaurantId}", restaurant.Id);
    }

    public async Task LogUpdateAsync(Guid restaurantId, Restaurant oldRestaurant, Restaurant newRestaurant, string username, ulong userId)
    {
        _logger.LogInformation("Logging update action for restaurant {RestaurantId} by {Username}", 
            restaurantId, username);

        var changes = new List<string>();
        var changeDetails = new Dictionary<string, object>();

        if (oldRestaurant.Name != newRestaurant.Name)
        {
            changes.Add($"Name: '{oldRestaurant.Name}' → '{newRestaurant.Name}'");
            changeDetails["Name"] = new { Old = oldRestaurant.Name, New = newRestaurant.Name };
        }

        if (oldRestaurant.City != newRestaurant.City)
        {
            changes.Add($"City: {oldRestaurant.City} → {newRestaurant.City}");
            changeDetails["City"] = new { Old = oldRestaurant.City.ToString(), New = newRestaurant.City.ToString() };
        }

        if (oldRestaurant.Url != newRestaurant.Url)
        {
            var oldUrl = string.IsNullOrWhiteSpace(oldRestaurant.Url) ? "(none)" : oldRestaurant.Url;
            var newUrl = string.IsNullOrWhiteSpace(newRestaurant.Url) ? "(removed)" : newRestaurant.Url;
            changes.Add($"URL: {oldUrl} → {newUrl}");
            changeDetails["Url"] = new { Old = oldRestaurant.Url, New = newRestaurant.Url };
        }

        if (changes.Count == 0)
        {
            _logger.LogWarning("No changes detected for restaurant {RestaurantId}", restaurantId);
            return;
        }

        var auditLog = new RestaurantAuditLog
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Action = AuditAction.Update,
            Timestamp = DateTime.UtcNow,
            Username = username,
            UserId = userId,
            ChangeDetails = JsonSerializer.Serialize(changeDetails),
            ChangeDescription = $"Updated restaurant '{oldRestaurant.Name}': {string.Join(", ", changes)}"
        };

        await _context.RestaurantAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully logged update action for restaurant {RestaurantId} with {ChangeCount} changes", 
            restaurantId, changes.Count);
    }

    public async Task LogDeleteAsync(Restaurant restaurant, string username, ulong userId)
    {
        _logger.LogInformation("Logging delete action for restaurant {RestaurantName} by {Username}", 
            restaurant.Name, username);

        var auditLog = new RestaurantAuditLog
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurant.Id,
            Action = AuditAction.Delete,
            Timestamp = DateTime.UtcNow,
            Username = username,
            UserId = userId,
            ChangeDetails = JsonSerializer.Serialize(new
            {
                restaurant.Id,
                restaurant.Name,
                City = restaurant.City.ToString(),
                restaurant.Url
            }),
            ChangeDescription = $"Deleted restaurant '{restaurant.Name}' from {restaurant.City}"
        };

        await _context.RestaurantAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully logged delete action for restaurant {RestaurantId}", restaurant.Id);
    }

    public async Task<IEnumerable<RestaurantAuditLog>> GetAuditHistoryAsync(Guid restaurantId)
    {
        _logger.LogInformation("Retrieving audit history for restaurant {RestaurantId}", restaurantId);

        return await _context.RestaurantAuditLogs
            .Where(log => log.RestaurantId == restaurantId)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestaurantAuditLog>> GetAllAuditLogsAsync(int pageSize, int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        }

        _logger.LogInformation("Retrieving audit logs (page: {PageNumber}, size: {PageSize})", 
            pageNumber, pageSize);

        var skip = (pageNumber - 1) * pageSize;

        return await _context.RestaurantAuditLogs
            .OrderByDescending(log => log.Timestamp)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }
}
