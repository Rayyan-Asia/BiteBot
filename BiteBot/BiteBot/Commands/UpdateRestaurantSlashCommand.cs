using Discord.Interactions;
using BiteBot.Models;
using BiteBot.Services;
using BiteBot.Interactions;
using Microsoft.Extensions.Logging;

namespace BiteBot.Commands;

public class UpdateRestaurantSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<UpdateRestaurantSlashCommand> _logger;

    public UpdateRestaurantSlashCommand(IRestaurantService restaurantService, ILogger<UpdateRestaurantSlashCommand> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [SlashCommand("update", "Update an existing restaurant")]
    public async Task UpdateAsync(
        [Summary("restaurant", "Select restaurant to update")]
        [Autocomplete(typeof(RestaurantAutocompleteHandler))] 
        string restaurantId,
        [Summary("name", "New restaurant name (leave empty to keep current)")] string? name = null,
        [Summary("city", "New city: -r/R for Ramallah, -n/N for Nablus (leave empty to keep current)")] string? cityOption = null,
        [Summary("url", "New restaurant URL (leave empty to keep current, use 'remove' to delete)")] string? url = null)
    {
        _logger.LogInformation("Update command invoked by {User} with restaurantId: {RestaurantId}", 
            Context.User.Username, restaurantId);

        await DeferAsync(ephemeral: true);

        try
        {
            if (!Guid.TryParse(restaurantId, out var id))
            {
                await RespondWithInvalidRestaurantError();
                return;
            }

            var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);

            var hasChanges = false;

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                restaurant.Name = name.Trim();
                hasChanges = true;
            }

            // Update city if provided
            if (!string.IsNullOrWhiteSpace(cityOption))
            {
                if (!TryParseCity(cityOption, out var city))
                {
                    await RespondWithInvalidCityError();
                    return;
                }
                restaurant.City = city;
                hasChanges = true;
            }

            // Update URL if provided
            if (url != null)
            {
                if (url.Trim().Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    restaurant.Url = null;
                    hasChanges = true;
                }
                else if (!string.IsNullOrWhiteSpace(url))
                {
                    if (!IsValidUrl(url))
                    {
                        await RespondWithInvalidUrlError();
                        return;
                    }
                    restaurant.Url = url.Trim();
                    hasChanges = true;
                }
            }

            if (!hasChanges)
            {
                await RespondWithNoChangesError();
                return;
            }

            var updatedRestaurant = await _restaurantService.UpsertRestaurantAsync(restaurant);
            
            await RespondWithSuccess(updatedRestaurant);
            
            _logger.LogInformation("Successfully updated restaurant {RestaurantName} (ID: {RestaurantId}) by {User}", 
                updatedRestaurant.Name, id, Context.User.Username);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Restaurant not found with ID: {RestaurantId}", restaurantId);
            await RespondWithNotFoundError();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant with ID: {RestaurantId}", restaurantId);
            
            if (ex.Message.Contains("duplicate") || ex.InnerException?.Message.Contains("duplicate") == true)
            {
                await RespondWithDuplicateError();
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

    private async Task RespondWithInvalidRestaurantError()
    {
        await FollowupAsync(
            "❌ Invalid restaurant selected. Please try again.",
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
            "❌ Invalid URL format. Please provide a valid HTTP or HTTPS URL, or use 'remove' to delete the URL.",
            ephemeral: true);
    }

    private async Task RespondWithNoChangesError()
    {
        await FollowupAsync(
            "❌ No changes were provided. Please specify at least one field to update.",
            ephemeral: true);
    }

    private async Task RespondWithNotFoundError()
    {
        await FollowupAsync(
            "❌ Restaurant not found. It may have been deleted.",
            ephemeral: true);
    }

    private async Task RespondWithDuplicateError()
    {
        await FollowupAsync(
            "❌ A restaurant with this name already exists in the specified city.",
            ephemeral: true);
    }

    private async Task RespondWithSuccess(Restaurant restaurant)
    {
        var message = $"✅ **Restaurant updated successfully!**\n\n" +
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
            "❌ An error occurred while updating the restaurant. Please try again later.",
            ephemeral: true);
    }
}

