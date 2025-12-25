using BiteBot.Models;

namespace BiteBot.Services;

public interface IAuditService
{
    /// <summary>
    /// Logs the creation of a restaurant
    /// </summary>
    Task LogCreateAsync(Restaurant restaurant, string username, ulong userId);
    
    /// <summary>
    /// Logs the update of a restaurant with details of what changed
    /// </summary>
    Task LogUpdateAsync(Guid restaurantId, Restaurant oldRestaurant, Restaurant newRestaurant, string username, ulong userId);
    
    /// <summary>
    /// Logs the deletion of a restaurant
    /// </summary>
    Task LogDeleteAsync(Restaurant restaurant, string username, ulong userId);
    
    /// <summary>
    /// Gets audit history for a specific restaurant
    /// </summary>
    Task<IEnumerable<RestaurantAuditLog>> GetAuditHistoryAsync(Guid restaurantId);
    
    /// <summary>
    /// Gets all audit logs with pagination
    /// </summary>
    Task<IEnumerable<RestaurantAuditLog>> GetAllAuditLogsAsync(int pageSize, int pageNumber = 1);
}