using System.ComponentModel.DataAnnotations;

namespace BiteBot.Models;

public class Restaurant
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; } = null!;
    
    [Required]
    public City City { get; set; }
    
    public string? Url { get; set; }
}