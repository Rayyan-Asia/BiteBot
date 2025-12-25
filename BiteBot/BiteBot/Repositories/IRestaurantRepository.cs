using BiteBot.Models;

namespace BiteBot.Repositories;

public interface IRestaurantRepository
{
    Task<Restaurant> UpsertRestaurant(Restaurant restaurant);
    Task<Restaurant> GetRestaurantById(Guid id);
    Task<Restaurant?> GetRandomRestaurantAsync(City city);
    Task<IEnumerable<Restaurant>> SearchRestaurantsByNameAndCityAsync(string name, City city, int pageSize, int pageNumber = 1);
    Task<IEnumerable<Restaurant>> SearchRestaurantsByNameAsync(string name, int pageSize, int pageNumber = 1);
    Task<IEnumerable<Restaurant>> GetAllByCityAsync(City city, int pageSize, int pageNumber = 1);
    Task DeleteRestaurantAsync(Guid id);
}