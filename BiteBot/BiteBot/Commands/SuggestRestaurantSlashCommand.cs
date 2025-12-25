using Discord.Interactions;
using BiteBot.Models;
using BiteBot.Services;
using Microsoft.Extensions.Logging;

namespace BiteBot.Commands;

public class SuggestRestaurantSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<SuggestRestaurantSlashCommand> _logger;

    public SuggestRestaurantSlashCommand(IRestaurantService restaurantService, ILogger<SuggestRestaurantSlashCommand> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [SlashCommand("suggest", "Get a random restaurant suggestion from a specific city")]
    public async Task SuggestAsync(
        [Summary("city", "City to get suggestion from: -r/R for Ramallah, -n/N for Nablus")] 
        string cityOption)
    {
        _logger.LogInformation("Suggest command invoked by {User} with city option: {CityOption}", 
            Context.User.Username, cityOption);

        await DeferAsync();

        try
        {
            if (!TryParseCity(cityOption, out var city))
            {
                await RespondWithInvalidCityError();
                return;
            }

            var restaurant = await _restaurantService.GetRandomRestaurantAsync(city);

            if (restaurant == null)
            {
                await RespondWithNoRestaurantsFound(city);
                return;
            }

            await RespondWithRestaurantSuggestion(restaurant, city);
            
            _logger.LogInformation("Suggested restaurant {RestaurantName} in {City} to {User}", 
                restaurant.Name, city, Context.User.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting restaurant for city option: {CityOption}", cityOption);
            await RespondWithGenericError();
        }
    }

    private bool TryParseCity(string cityOption, out City city)
    {
        var normalizedOption = cityOption.Trim().ToLower();
        
        switch (normalizedOption)
        {
            case "-r":
            case "r":
                city = City.Ramallah;
                return true;
            case "-n":
            case "n":
                city = City.Nablus;
                return true;
            default:
                city = default;
                return false;
        }
    }

    private async Task RespondWithInvalidCityError()
    {
        await FollowupAsync(
            "❌ Invalid city option. Please use:\n" +
            "• **-r** or **R** for Ramallah\n" +
            "• **-n** or **N** for Nablus",
            ephemeral: true);
    }

    private async Task RespondWithNoRestaurantsFound(City city)
    {
        await FollowupAsync(
            $"😔 No restaurants found in **{city}**. Please add some restaurants first!",
            ephemeral: true);
    }

    private async Task RespondWithRestaurantSuggestion(Restaurant restaurant, City city)
    {
        var response = BuildRestaurantSuggestionMessage(restaurant, city);
        await FollowupAsync(response);
    }

    private string BuildRestaurantSuggestionMessage(Restaurant restaurant, City city)
    {
        var message = $"🍽️ **Restaurant Suggestion for {city}**\n\n" +
                      $"**{restaurant.Name}**\n";

        if (!string.IsNullOrWhiteSpace(restaurant.Url))
        {
            message += $"🔗 {restaurant.Url}";
        }

        return message;
    }

    private async Task RespondWithGenericError()
    {
        await FollowupAsync(
            "❌ An error occurred while fetching a restaurant suggestion. Please try again later.",
            ephemeral: true);
    }
}

