using System.Text.Json;
using BiteBot.Data;
using BiteBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BiteBot.Services;

public class AuditService(AppDbContext context, ILogger<AuditService> logger) : IAuditService
{
    public async Task LogCreateAsync(Restaurant restaurant, string username, ulong userId)
    {
        logger.LogInformation("Logging create action for restaurant {RestaurantName} by {Username}", 
            restaurant.Name, username);

        var changeDetails = SerializeRestaurantDetails(restaurant);
        var changeDescription = BuildCreateDescription(restaurant);
        var auditLog = CreateAuditLog(restaurant.Id, AuditAction.Create, username, userId, changeDetails, changeDescription);

        await SaveAuditLogAsync(auditLog);
        
        logger.LogInformation("Successfully logged create action for restaurant {RestaurantId}", restaurant.Id);
    }

    public async Task LogUpdateAsync(Guid restaurantId, Restaurant oldRestaurant, Restaurant newRestaurant, string username, ulong userId)
    {
        logger.LogInformation("Logging update action for restaurant {RestaurantId} by {Username}", 
            restaurantId, username);

        var (changes, changeDetails) = DetectRestaurantChanges(oldRestaurant, newRestaurant);

        if (!HasChanges(changes, restaurantId))
        {
            return;
        }

        var changeDetailsJson = JsonSerializer.Serialize(changeDetails);
        var changeDescription = BuildUpdateDescription(oldRestaurant.Name, changes);
        var auditLog = CreateAuditLog(restaurantId, AuditAction.Update, username, userId, changeDetailsJson, changeDescription);

        await SaveAuditLogAsync(auditLog);
        
        logger.LogInformation("Successfully logged update action for restaurant {RestaurantId} with {ChangeCount} changes", 
            restaurantId, changes.Count);
    }

    public async Task LogDeleteAsync(Restaurant restaurant, string username, ulong userId)
    {
        logger.LogInformation("Logging delete action for restaurant {RestaurantName} by {Username}", 
            restaurant.Name, username);

        var changeDetails = SerializeRestaurantDetails(restaurant);
        var changeDescription = BuildDeleteDescription(restaurant);
        var auditLog = CreateAuditLog(restaurant.Id, AuditAction.Delete, username, userId, changeDetails, changeDescription);

        await SaveAuditLogAsync(auditLog);
        
        logger.LogInformation("Successfully logged delete action for restaurant {RestaurantId}", restaurant.Id);
    }

    public async Task<IEnumerable<RestaurantAuditLog>> GetAuditHistoryAsync(Guid restaurantId)
    {
        logger.LogInformation("Retrieving audit history for restaurant {RestaurantId}", restaurantId);

        return await context.RestaurantAuditLogs
            .Where(log => log.RestaurantId == restaurantId)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestaurantAuditLog>> GetAllAuditLogsAsync(int pageSize, int pageNumber = 1)
    {
        ValidatePageNumber(pageNumber);

        logger.LogInformation("Retrieving audit logs (page: {PageNumber}, size: {PageSize})", 
            pageNumber, pageSize);

        var skip = CalculateSkip(pageNumber, pageSize);

        return await context.RestaurantAuditLogs
            .OrderByDescending(log => log.Timestamp)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    // Private helper methods

    private string SerializeRestaurantDetails(Restaurant restaurant)
    {
        return JsonSerializer.Serialize(new
        {
            restaurant.Id,
            restaurant.Name,
            City = restaurant.City.ToString(),
            restaurant.Url
        });
    }

    private string BuildCreateDescription(Restaurant restaurant)
    {
        var description = $"Created restaurant '{restaurant.Name}' in {restaurant.City}";
        if (!string.IsNullOrWhiteSpace(restaurant.Url))
        {
            description += $" with URL: {restaurant.Url}";
        }
        return description;
    }

    private string BuildDeleteDescription(Restaurant restaurant)
    {
        return $"Deleted restaurant '{restaurant.Name}' from {restaurant.City}";
    }

    private string BuildUpdateDescription(string restaurantName, List<string> changes)
    {
        return $"Updated restaurant '{restaurantName}': {string.Join(", ", changes)}";
    }

    private (List<string> changes, Dictionary<string, object> changeDetails) DetectRestaurantChanges(
        Restaurant oldRestaurant, 
        Restaurant newRestaurant)
    {
        var changes = new List<string>();
        var changeDetails = new Dictionary<string, object>();

        // Get all properties we want to track (exclude navigation properties and Id)
        var propertiesToTrack = new[] { "Name", "City", "Url" };

        foreach (var propertyName in propertiesToTrack)
        {
            var property = typeof(Restaurant).GetProperty(propertyName);
            if (property == null) continue;

            var oldValue = property.GetValue(oldRestaurant);
            var newValue = property.GetValue(newRestaurant);

            if (!AreValuesEqual(oldValue, newValue))
            {
                var changeDescription = FormatChangeDescription(propertyName, oldValue, newValue);
                changes.Add(changeDescription);
                changeDetails[propertyName] = new { Old = FormatValueForStorage(oldValue), New = FormatValueForStorage(newValue) };
            }
        }

        return (changes, changeDetails);
    }

    private static bool AreValuesEqual(object? oldValue, object? newValue)
    {
        if (oldValue == null && newValue == null) return true;
        if (oldValue == null || newValue == null) return false;
        return oldValue.Equals(newValue);
    }

    private static string FormatChangeDescription(string propertyName, object? oldValue, object? newValue)
    {
        var formattedOld = FormatValueForDisplay(oldValue);
        var formattedNew = FormatValueForDisplay(newValue);

        return propertyName switch
        {
            "Name" => $"Name: '{formattedOld}' → '{formattedNew}'",
            "Url" => $"URL: {formattedOld} → {formattedNew}",
            _ => $"{propertyName}: {formattedOld} → {formattedNew}"
        };
    }

    private static string FormatValueForDisplay(object? value)
    {
        return value switch
        {
            null => "(none)",
            string str when string.IsNullOrWhiteSpace(str) => "(none)",
            _ => value.ToString() ?? "(none)"
        };
    }

    private static object? FormatValueForStorage(object? value)
    {
        return value switch
        {
            City city => city.ToString(),
            _ => value
        };
    }

    private bool HasChanges(List<string> changes, Guid restaurantId)
    {
        if (changes.Count != 0) return true;
        logger.LogWarning("No changes detected for restaurant {RestaurantId}", restaurantId);
        return false;
    }

    private static RestaurantAuditLog CreateAuditLog(
        Guid restaurantId, 
        AuditAction action, 
        string username, 
        ulong userId, 
        string changeDetails, 
        string changeDescription)
    {
        return new RestaurantAuditLog
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            Username = username,
            UserId = userId,
            ChangeDetails = changeDetails,
            ChangeDescription = changeDescription
        };
    }

    private async Task SaveAuditLogAsync(RestaurantAuditLog auditLog)
    {
        await context.RestaurantAuditLogs.AddAsync(auditLog);
        await context.SaveChangesAsync();
    }

    private static void ValidatePageNumber(int pageNumber)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
        }
    }

    private static int CalculateSkip(int pageNumber, int pageSize)
    {
        return (pageNumber - 1) * pageSize;
    }
}
