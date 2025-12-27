using Discord.Interactions;
using BiteBot.Models;
using BiteBot.Services;
using BiteBot.Helpers;
using Microsoft.Extensions.Logging;

namespace BiteBot.Commands;

public class AddRestaurantSlashCommand(
    IRestaurantService restaurantService,
    IAuditService auditService,
    ILogger<AddRestaurantSlashCommand> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add", "Add a new restaurant")]
    public async Task AddAsync(
        [Summary("name", "Restaurant name")] string name,
        [Summary("city", "City: -r/R for Ramallah, -n/N for Nablus")] string cityOption,
        [Summary("url", "Optional restaurant URL")] string? url = null)
    {
        logger.LogInformation("Add command invoked by {User} with name: {Name}, city: {CityOption}, url: {Url}", 
            Context.User.Username, name, cityOption, url);

        await DeferAsync(ephemeral: true);

        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await RespondWithInvalidNameError();
                return;
            }

            if (!ValidationHelper.TryParseCity(cityOption, out var city))
            {
                await RespondWithInvalidCityError();
                return;
            }

            if (!string.IsNullOrWhiteSpace(url) && !ValidationHelper.IsValidUrl(url))
            {
                await RespondWithInvalidUrlError();
                return;
            }

            var restaurant = await CreateRestaurant(name, city, url);
            
            // Log the audit trail
            await auditService.LogCreateAsync(restaurant, Context.User.Username, Context.User.Id);
            
            await RespondWithSuccess(restaurant);
            
            logger.LogInformation("Successfully added restaurant {RestaurantName} in {City} by {User}", 
                restaurant.Name, city, Context.User.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding restaurant with name: {Name}, city: {CityOption}", name, cityOption);
            
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


    private async Task<Restaurant> CreateRestaurant(string name, City city, string? url)
    {
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            City = city,
            Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim()
        };

        return await restaurantService.UpsertRestaurantAsync(restaurant);
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

