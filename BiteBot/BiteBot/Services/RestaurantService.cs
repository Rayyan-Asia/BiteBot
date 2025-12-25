using BiteBot.Models;
using BiteBot.Repositories;
using Microsoft.Extensions.Logging;

namespace BiteBot.Services;

public class RestaurantService(IRestaurantRepository repository, ILogger<RestaurantService> logger) : IRestaurantService
{
    public async Task<Restaurant> UpsertRestaurantAsync(Restaurant restaurant)
    {
        logger.LogInformation("Upserting restaurant: {RestaurantName} in {City}", restaurant.Name, restaurant.City);
        
        try
        {
            var result = await repository.UpsertRestaurant(restaurant);
            logger.LogInformation("Successfully upserted restaurant with ID: {RestaurantId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting restaurant: {RestaurantName}", restaurant.Name);
            throw;
        }
    }

    public async Task<Restaurant> GetRestaurantByIdAsync(Guid id)
    {
        logger.LogInformation("Fetching restaurant with ID: {RestaurantId}", id);
        
        try
        {
            return await repository.GetRestaurantById(id);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Restaurant with ID {RestaurantId} not found", id);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching restaurant with ID: {RestaurantId}", id);
            throw;
        }
    }

    public async Task<Restaurant?> GetRandomRestaurantAsync(City city)
    {
        logger.LogInformation("Fetching random restaurant in {City}", city);
        
        try
        {
            var result = await repository.GetRandomRestaurantAsync(city);
            
            if (result == null)
            {
                logger.LogInformation("No restaurants found in {City}", city);
            }
            else
            {
                logger.LogInformation("Found random restaurant: {RestaurantName} in {City}", result.Name, city);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching random restaurant in {City}", city);
            throw;
        }
    }

    public async Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(string name, City city, int pageSize, int pageNumber = 1)
    {
        logger.LogInformation("Searching for restaurants with name containing '{Name}' in {City} (page: {PageNumber}, size: {PageSize})", 
            name, city, pageNumber, pageSize);
        
        try
        {
            var results = await repository.SearchRestaurantsByNameAndCityAsync(name, city, pageSize, pageNumber);
            var resultList = results.ToList();
            logger.LogInformation("Found {Count} restaurants matching search criteria", resultList.Count);
            return resultList;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid search criteria");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching restaurants");
            throw;
        }
    }

    public async Task<IEnumerable<Restaurant>> SearchRestaurantsByNameAsync(string name, int pageSize, int pageNumber = 1)
    {
        logger.LogInformation("Searching for restaurants with name containing '{Name}' across all cities (page: {PageNumber}, size: {PageSize})", 
            name, pageNumber, pageSize);
        
        try
        {
            var results = await repository.SearchRestaurantsByNameAsync(name, pageSize, pageNumber);
            var resultList = results.ToList();
            logger.LogInformation("Found {Count} restaurants matching search criteria", resultList.Count);
            return resultList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching restaurants by name");
            throw;
        }
    }

    public async Task DeleteRestaurantAsync(Guid id)
    {
        logger.LogInformation("Deleting restaurant with ID: {RestaurantId}", id);
        
        try
        {
            await repository.DeleteRestaurantAsync(id);
            logger.LogInformation("Successfully deleted restaurant with ID: {RestaurantId}", id);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Restaurant with ID {RestaurantId} not found for deletion", id);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting restaurant with ID: {RestaurantId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Restaurant>> GetRestaurantsByCityAsync(City city, int pageSize, int pageNumber = 1)
    {
        logger.LogInformation("Fetching all restaurants in {City} (page: {PageNumber}, size: {PageSize})", 
            city, pageNumber, pageSize);
        
        try
        {
            var results = await repository.GetAllByCityAsync(city, pageSize, pageNumber);
            var resultList = results.ToList();
            logger.LogInformation("Found {Count} restaurants in {City}", resultList.Count, city);
            return resultList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching restaurants in {City}", city);
            throw;
        }
    }
}
