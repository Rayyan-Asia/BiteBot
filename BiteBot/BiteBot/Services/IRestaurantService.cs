using BiteBot.Models;

namespace BiteBot.Services;

public interface IRestaurantService
{
    /// <summary>
    /// Creates a new restaurant or updates an existing one
    /// </summary>
    Task<Restaurant> UpsertRestaurantAsync(Restaurant restaurant);
    
    /// <summary>
    /// Gets a restaurant by its unique identifier
    /// </summary>
    Task<Restaurant> GetRestaurantByIdAsync(Guid id);
    
    /// <summary>
    /// Gets a random restaurant from a specific city
    /// </summary>
    Task<Restaurant?> GetRandomRestaurantAsync(City city);
    
    /// <summary>
    /// Searches for restaurants by name within a specific city
    /// </summary>
    Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(string name, City city);
    
    /// <summary>
    /// Deletes a restaurant by its unique identifier
    /// </summary>
    Task DeleteRestaurantAsync(Guid id);
    
    /// <summary>
    /// Gets all restaurants in a specific city
    /// </summary>
    Task<IEnumerable<Restaurant>> GetRestaurantsByCityAsync(City city);
}

