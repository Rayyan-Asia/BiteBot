using BiteBot.Models;

namespace BiteBot.Repositories;

public interface IRestaurantRepository
{
    Task<Restaurant> UpsertRestaurant(Restaurant restaurant);
    Task<Restaurant> GetRestaurantById(Guid id);
    Task<Restaurant?> GetRandomRestaurantAsync(City city);
    Task<IEnumerable<Restaurant>> SearchRestaurantsByNameAndCityAsync(string name, City city);
    Task<IEnumerable<Restaurant>> GetAllByCityAsync(City city);
    Task DeleteRestaurantAsync(Guid id);
}