using BiteBot.Models;
using BiteBot.Repositories;
using Microsoft.Extensions.Logging;

namespace BiteBot.Services;

public class RestaurantService(IRestaurantRepository repository, ILogger<RestaurantService> logger) : IRestaurantService
{
    public async Task<Restaurant> UpsertRestaurantAsync(Restaurant restaurant)
    {
        return await ExecuteWithLoggingAsync(
            async () => await repository.UpsertRestaurant(restaurant),
            () => logger.LogInformation("Upserting restaurant: {RestaurantName} in {City}", restaurant.Name, restaurant.City),
            result => logger.LogInformation("Successfully upserted restaurant with ID: {RestaurantId}", result.Id),
            ex => logger.LogError(ex, "Error upserting restaurant: {RestaurantName}", restaurant.Name)
        );
    }

    public async Task<Restaurant> GetRestaurantByIdAsync(Guid id)
    {
        return await ExecuteWithNotFoundHandlingAsync(
            async () => await repository.GetRestaurantById(id),
            () => logger.LogInformation("Fetching restaurant with ID: {RestaurantId}", id),
            ex => logger.LogWarning(ex, "Restaurant with ID {RestaurantId} not found", id),
            ex => logger.LogError(ex, "Error fetching restaurant with ID: {RestaurantId}", id)
        );
    }

    public async Task<Restaurant?> GetRandomRestaurantAsync(City city)
    {
        return await ExecuteWithLoggingAsync(
            async () => await repository.GetRandomRestaurantAsync(city),
            () => logger.LogInformation("Fetching random restaurant in {City}", city),
            result => LogRandomRestaurantResult(result, city),
            ex => logger.LogError(ex, "Error fetching random restaurant in {City}", city)
        );
    }

    public async Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(string name, City city, int pageSize, int pageNumber = 1)
    {
        return await ExecuteSearchWithLoggingAsync(
            async () => await repository.SearchRestaurantsByNameAndCityAsync(name, city, pageSize, pageNumber),
            () => logger.LogInformation("Searching for restaurants with name containing '{Name}' in {City} (page: {PageNumber}, size: {PageSize})", 
                name, city, pageNumber, pageSize),
            ex => logger.LogWarning(ex, "Invalid search criteria"),
            ex => logger.LogError(ex, "Error searching restaurants")
        );
    }

    public async Task<IEnumerable<Restaurant>> SearchRestaurantsByNameAsync(string name, int pageSize, int pageNumber = 1)
    {
        return await ExecuteCollectionWithLoggingAsync(
            async () => await repository.SearchRestaurantsByNameAsync(name, pageSize, pageNumber),
            () => logger.LogInformation("Searching for restaurants with name containing '{Name}' across all cities (page: {PageNumber}, size: {PageSize})", 
                name, pageNumber, pageSize),
            count => logger.LogInformation("Found {Count} restaurants matching search criteria", count),
            ex => logger.LogError(ex, "Error searching restaurants by name")
        );
    }

    public async Task DeleteRestaurantAsync(Guid id)
    {
        await ExecuteWithVoidLoggingAsync(
            async () => await repository.DeleteRestaurantAsync(id),
            () => logger.LogInformation("Deleting restaurant with ID: {RestaurantId}", id),
            () => logger.LogInformation("Successfully deleted restaurant with ID: {RestaurantId}", id),
            ex => logger.LogWarning(ex, "Restaurant with ID {RestaurantId} not found for deletion", id),
            ex => logger.LogError(ex, "Error deleting restaurant with ID: {RestaurantId}", id)
        );
    }

    public async Task<IEnumerable<Restaurant>> GetRestaurantsByCityAsync(City city, int pageSize, int pageNumber = 1)
    {
        return await ExecuteCollectionWithLoggingAsync(
            async () => await repository.GetAllByCityAsync(city, pageSize, pageNumber),
            () => logger.LogInformation("Fetching all restaurants in {City} (page: {PageNumber}, size: {PageSize})", 
                city, pageNumber, pageSize),
            count => logger.LogInformation("Found {Count} restaurants in {City}", count, city),
            ex => logger.LogError(ex, "Error fetching restaurants in {City}", city)
        );
    }

    // Private helper methods for common patterns

    private async Task<T> ExecuteWithLoggingAsync<T>(
        Func<Task<T>> operation,
        Action logStart,
        Action<T> logSuccess,
        Action<Exception> logError)
    {
        logStart();
        
        try
        {
            var result = await operation();
            logSuccess(result);
            return result;
        }
        catch (Exception ex)
        {
            logError(ex);
            throw;
        }
    }

    private async Task<T> ExecuteWithNotFoundHandlingAsync<T>(
        Func<Task<T>> operation,
        Action logStart,
        Action<Exception> logNotFound,
        Action<Exception> logError)
    {
        logStart();
        
        try
        {
            return await operation();
        }
        catch (KeyNotFoundException ex)
        {
            logNotFound(ex);
            throw;
        }
        catch (Exception ex)
        {
            logError(ex);
            throw;
        }
    }

    private async Task<IEnumerable<Restaurant>> ExecuteSearchWithLoggingAsync(
        Func<Task<IEnumerable<Restaurant>>> operation,
        Action logStart,
        Action<Exception> logArgumentError,
        Action<Exception> logError)
    {
        logStart();
        
        try
        {
            var results = await operation();
            var resultList = results.ToList();
            logger.LogInformation("Found {Count} restaurants matching search criteria", resultList.Count);
            return resultList;
        }
        catch (ArgumentException ex)
        {
            logArgumentError(ex);
            throw;
        }
        catch (Exception ex)
        {
            logError(ex);
            throw;
        }
    }

    private async Task<IEnumerable<Restaurant>> ExecuteCollectionWithLoggingAsync(
        Func<Task<IEnumerable<Restaurant>>> operation,
        Action logStart,
        Action<int> logSuccess,
        Action<Exception> logError)
    {
        logStart();
        
        try
        {
            var results = await operation();
            var resultList = results.ToList();
            logSuccess(resultList.Count);
            return resultList;
        }
        catch (Exception ex)
        {
            logError(ex);
            throw;
        }
    }

    private async Task ExecuteWithVoidLoggingAsync(
        Func<Task> operation,
        Action logStart,
        Action logSuccess,
        Action<Exception> logNotFound,
        Action<Exception> logError)
    {
        logStart();
        
        try
        {
            await operation();
            logSuccess();
        }
        catch (KeyNotFoundException ex)
        {
            logNotFound(ex);
            throw;
        }
        catch (Exception ex)
        {
            logError(ex);
            throw;
        }
    }

    private void LogRandomRestaurantResult(Restaurant? result, City city)
    {
        if (result == null)
        {
            logger.LogInformation("No restaurants found in {City}", city);
        }
        else
        {
            logger.LogInformation("Found random restaurant: {RestaurantName} in {City}", result.Name, city);
        }
    }
}
