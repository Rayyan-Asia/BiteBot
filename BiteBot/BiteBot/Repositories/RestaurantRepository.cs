using BiteBot.Data;
using BiteBot.Models;
using Microsoft.EntityFrameworkCore;

namespace BiteBot.Repositories;

public class RestaurantRepository(AppDbContext context) : IRestaurantRepository
{
    public async Task<Restaurant> UpsertRestaurant(Restaurant restaurant)
    {
        var existing = await context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == restaurant.Id);
        
        if (existing != null)
        {
            // Update existing restaurant
            existing.Name = restaurant.Name;
            existing.City = restaurant.City;
            existing.Url = restaurant.Url;
            context.Restaurants.Update(existing);
        }
        else
        {
            // Insert new restaurant
            restaurant = (await context.Restaurants.AddAsync(restaurant)).Entity;
        }
        
        await context.SaveChangesAsync();
        return existing ?? restaurant;
    }

    public async Task<Restaurant> GetRestaurantById(Guid id)
    {
        return await context.Restaurants.FirstOrDefaultAsync(r => r.Id == id) ??
               throw new KeyNotFoundException($"Restaurant with ID '{id}' was not found.");
    }

    public async Task<Restaurant?> GetRandomRestaurantAsync(City city)
    {
        var count = await context.Restaurants
            .Where(r => r.City == city)
            .CountAsync();

        if (count == 0) return null;

        var randomIndex = Random.Shared.Next(count);

        return await context.Restaurants
            .Where(r => r.City == city)
            .Skip(randomIndex)
            .FirstAsync();
    }

    public async Task<IEnumerable<Restaurant>> SearchRestaurantsByNameAndCityAsync(string name, City city)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Restaurant name cannot be null or empty.", nameof(name));
        }
        
        return await context.Restaurants
            .Where(r => r.City == city && r.Name.Contains(name))
            .ToListAsync();
    }

    public async Task<IEnumerable<Restaurant>> GetAllByCityAsync(City city)
    {
        return await context.Restaurants
            .Where(r => r.City == city)
            .ToListAsync();
    }

    public async Task DeleteRestaurantAsync(Guid id)
    {
        var restaurant = await context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with ID '{id}' was not found.");
        }
        
        context.Restaurants.Remove(restaurant);
        await context.SaveChangesAsync();
    }
}