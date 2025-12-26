using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using BiteBot.Services;
using BiteBot.Interactions;
using Microsoft.Extensions.Logging;

namespace BiteBot.Commands;

public class OrderSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<OrderSlashCommand> _logger;

    public OrderSlashCommand(
        IRestaurantService restaurantService,
        ILogger<OrderSlashCommand> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [SlashCommand("order", "Create an order thread for a restaurant")]
    public async Task OrderAsync(
        [Summary("restaurant", "Select a restaurant to order from")]
        [Autocomplete(typeof(RestaurantAutocompleteHandler))] 
        string restaurantId)
    {
        _logger.LogInformation("Order command invoked by {User} with restaurantId: {RestaurantId}", 
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

            // Get the channel where the command was invoked
            if (Context.Channel is not SocketTextChannel textChannel)
            {
                await RespondWithInvalidChannelError();
                return;
            }

            // Create a thread with the restaurant name as the title
            var thread = await textChannel.CreateThreadAsync(
                name: restaurant.Name,
                type: ThreadType.PublicThread,
                autoArchiveDuration: ThreadArchiveDuration.OneDay,
                message: null,
                invitable: true);

            _logger.LogInformation("Created thread {ThreadName} (ID: {ThreadId}) for restaurant {RestaurantName}", 
                thread.Name, thread.Id, restaurant.Name);

            // Build the order message
            var orderMessage = BuildOrderMessage(restaurant);

            // Send the message to the thread
            await thread.SendMessageAsync(orderMessage);

            // Respond to the user
            await RespondWithSuccess(restaurant, thread);
            
            _logger.LogInformation("Successfully created order thread for {RestaurantName} by {User}", 
                restaurant.Name, Context.User.Username);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Restaurant not found with ID: {RestaurantId}", restaurantId);
            await RespondWithNotFoundError();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order thread for restaurant ID: {RestaurantId}", restaurantId);
            await RespondWithGenericError();
        }
    }

    private string BuildOrderMessage(Models.Restaurant restaurant)
    {
        var message = $"🍽️ **We would like to order from {restaurant.Name}. Please put your order below!**\n\n";
        
        if (!string.IsNullOrWhiteSpace(restaurant.Url))
        {
            message += $"🔗 **Menu/Website:** {restaurant.Url}\n\n";
        }
        
        message += $"📍 **Location:** {restaurant.City}\n\n";
        message += "👇 **Post your orders as replies to this thread!**";

        return message;
    }

    private async Task RespondWithInvalidRestaurantError()
    {
        await FollowupAsync(
            "❌ Invalid restaurant selected. Please try again.",
            ephemeral: true);
    }

    private async Task RespondWithInvalidChannelError()
    {
        await FollowupAsync(
            "❌ This command can only be used in text channels.",
            ephemeral: true);
    }

    private async Task RespondWithNotFoundError()
    {
        await FollowupAsync(
            "❌ Restaurant not found. It may have been deleted.",
            ephemeral: true);
    }

    private async Task RespondWithSuccess(Models.Restaurant restaurant, SocketThreadChannel thread)
    {
        var message = $"✅ **Order thread created successfully!**\n\n" +
                      $"📍 **Restaurant:** {restaurant.Name}\n" +
                      $"🧵 **Thread:** <#{thread.Id}>\n\n" +
                      $"The thread has been created and everyone can now place their orders!";

        await FollowupAsync(message, ephemeral: true);
    }

    private async Task RespondWithGenericError()
    {
        await FollowupAsync(
            "❌ An error occurred while creating the order thread. Please try again later.",
            ephemeral: true);
    }
}

