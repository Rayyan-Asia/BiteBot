using Discord.Interactions;
using BiteBot.Services;
using BiteBot.Interactions;
using Microsoft.Extensions.Logging;

namespace BiteBot.Commands;

public class DeleteRestaurantSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<DeleteRestaurantSlashCommand> _logger;

    public DeleteRestaurantSlashCommand(IRestaurantService restaurantService, ILogger<DeleteRestaurantSlashCommand> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [SlashCommand("delete", "Delete a restaurant from the database")]
    public async Task DeleteAsync(
        [Summary("restaurant", "Select the restaurant to delete")]
        [Autocomplete(typeof(RestaurantAutocompleteHandler))]
        string restaurantId)
    {
        _logger.LogInformation("Delete command invoked by {User} for restaurant ID: {RestaurantId}", 
            Context.User.Username, restaurantId);

        await DeferAsync(ephemeral: true);

        try
        {
            if (!TryParseRestaurantId(restaurantId, out var id))
            {
                await RespondWithInvalidIdError();
                return;
            }

            var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
            await _restaurantService.DeleteRestaurantAsync(id);
            await RespondWithDeleteSuccess(restaurant.Name, restaurant.City.ToString());
            
            _logger.LogInformation("Successfully deleted restaurant {RestaurantName} (ID: {RestaurantId}) by user {User}", 
                restaurant.Name, id, Context.User.Username);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Restaurant with ID {RestaurantId} not found for deletion", restaurantId);
            await RespondWithRestaurantNotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant with ID: {RestaurantId}", restaurantId);
            await RespondWithGenericError();
        }
    }

    private bool TryParseRestaurantId(string restaurantId, out Guid id)
    {
        return Guid.TryParse(restaurantId, out id);
    }

    private async Task RespondWithInvalidIdError()
    {
        await FollowupAsync(
            "❌ Invalid restaurant ID format. Please select a restaurant from the autocomplete suggestions.",
            ephemeral: true);
    }

    private async Task RespondWithRestaurantNotFound()
    {
        await FollowupAsync(
            "❌ Restaurant not found. It may have already been deleted.",
            ephemeral: true);
    }

    private async Task RespondWithDeleteSuccess(string restaurantName, string city)
    {
        await FollowupAsync(
            $"✅ Successfully deleted **{restaurantName}** from **{city}**.",
            ephemeral: true);
    }

    private async Task RespondWithGenericError()
    {
        await FollowupAsync(
            "❌ An error occurred while deleting the restaurant. Please try again later.",
            ephemeral: true);
    }
}

