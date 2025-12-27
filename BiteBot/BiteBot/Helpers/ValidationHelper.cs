using BiteBot.Models;

namespace BiteBot.Helpers;

/// <summary>
/// Provides common validation and parsing utilities for Discord commands
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates if a string is a valid HTTP or HTTPS URL
    /// </summary>
    /// <param name="url">The URL string to validate</param>
    /// <returns>True if the URL is valid, false otherwise</returns>
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Attempts to parse a city option string into a City enum value
    /// Supports: -r/R for Ramallah, -n/N for Nablus
    /// </summary>
    /// <param name="cityOption">The city option string to parse</param>
    /// <param name="city">The parsed City enum value if successful</param>
    /// <returns>True if parsing was successful, false otherwise</returns>
    public static bool TryParseCity(string cityOption, out City city)
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
}

