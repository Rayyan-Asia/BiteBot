using BiteBot.Services;
using BiteBot.Constants;

namespace BiteBot.Interactions;

using Discord;
using Discord.Interactions;

public sealed class RestaurantAutocompleteHandler : AutocompleteHandler
{
    // AutocompleteHandlers are singleton-cached in Discord.Net’s InteractionService. :contentReference[oaicite:10]{index=10}
    // So inject only singleton-safe dependencies here (e.g., IDbContextFactory or a thread-safe repo).
    private readonly IRestaurantService _restaurants;

    public RestaurantAutocompleteHandler(IRestaurantService restaurants)
        => _restaurants = restaurants;

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        // This is the text the user is currently typing for the focused option.
        var userInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

        // Optional UX rule: don't query DB until they type 2+ chars
        if (userInput.Length < 2)
            return AutocompletionResult.FromSuccess(); // Shows “No options match…” in client

        var matches = await _restaurants.SearchRestaurantsByNameAsync(userInput, DiscordConstants.AutocompleteMaxResults);

        // name = label shown in the dropdown
        // value = what your slash command receives
        // Keep value short (<=100 chars for string). :contentReference[oaicite:11]{index=11}
        var results = matches.Select(r =>
            new AutocompleteResult(
                name: $"{r.Name} — {r.City.ToString()}",
                value: r.Id.ToString()
            )
        );

        return AutocompletionResult.FromSuccess(results);
    }
}
