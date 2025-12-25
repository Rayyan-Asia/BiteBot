using Discord.Interactions;
using BiteBot.Models;
using BiteBot.Services;
using Microsoft.Extensions.Logging;

namespace BiteBot.Commands;

public class AddRestaurantSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<AddRestaurantSlashCommand> _logger;

    public AddRestaurantSlashCommand(IRestaurantService restaurantService, ILogger<AddRestaurantSlashCommand> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [SlashCommand("add", "Add a new restaurant")]
    public async Task AddAsync(
        [Summary("name", "Restaurant name")] string name,
        [Summary("city", "City: -r/R for Ramallah, -n/N for Nablus")] string cityOption,
        [Summary("url", "Optional restaurant URL")] string? url = null)
    {
        _logger.LogInformation("Add command invoked by {User} with name: {Name}, city: {CityOption}, url: {Url}", 
            Context.User.Username, name, cityOption, url);

        await DeferAsync(ephemeral: true);

        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await RespondWithInvalidNameError();
                return;
            }

            if (!TryParseCity(cityOption, out var city))
            {
                await RespondWithInvalidCityError();
                return;
            }

            if (!string.IsNullOrWhiteSpace(url) && !IsValidUrl(url))
            {
                await RespondWithInvalidUrlError();
                return;
            }

            var restaurant = await CreateRestaurant(name, city, url);
            
            await RespondWithSuccess(restaurant);
            
            _logger.LogInformation("Successfully added restaurant {RestaurantName} in {City} by {User}", 
                restaurant.Name, city, Context.User.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding restaurant with name: {Name}, city: {CityOption}", name, cityOption);
            
            if (ex.Message.Contains("duplicate") || ex.InnerException?.Message.Contains("duplicate") == true)
            {
                await RespondWithDuplicateError(name);
            }
            else
            {
                await RespondWithGenericError();
            }
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

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async Task<Restaurant> CreateRestaurant(string name, City city, string? url)
    {
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            City = city,
            Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim()
        };

        return await _restaurantService.UpsertRestaurantAsync(restaurant);
    }

    private async Task RespondWithInvalidNameError()
    {
        await FollowupAsync(
            "❌ Restaurant name cannot be empty.",
            ephemeral: true);
    }

    private async Task RespondWithInvalidCityError()
    {
        await FollowupAsync(
            "❌ Invalid city option. Please use:\n" +
            "• **-r** or **R** for Ramallah\n" +
            "• **-n** or **N** for Nablus",
            ephemeral: true);
    }

    private async Task RespondWithInvalidUrlError()
    {
        await FollowupAsync(
            "❌ Invalid URL format. Please provide a valid HTTP or HTTPS URL.",
            ephemeral: true);
    }

    private async Task RespondWithDuplicateError(string name)
    {
        await FollowupAsync(
            $"❌ A restaurant with the name **{name}** already exists in this city. Use `/update` to modify it instead.",
            ephemeral: true);
    }

    private async Task RespondWithSuccess(Restaurant restaurant)
    {
        var message = $"✅ **Restaurant added successfully!**\n\n" +
                      $"📍 **Name:** {restaurant.Name}\n" +
                      $"🏙️ **City:** {restaurant.City}";

        if (!string.IsNullOrWhiteSpace(restaurant.Url))
        {
            message += $"\n🔗 **URL:** {restaurant.Url}";
        }

        await FollowupAsync(message, ephemeral: true);
    }

    private async Task RespondWithGenericError()
    {
        await FollowupAsync(
            "❌ An error occurred while adding the restaurant. Please try again later.",
            ephemeral: true);
    }
}

