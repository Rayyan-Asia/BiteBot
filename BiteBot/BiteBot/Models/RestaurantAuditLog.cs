using System.ComponentModel.DataAnnotations;

namespace BiteBot.Models;

public class RestaurantAuditLog
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid RestaurantId { get; set; }
    
    [Required]
    public AuditAction Action { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    public string Username { get; set; } = null!;
    
    [Required]
    public ulong UserId { get; set; }
    
    /// <summary>
    /// JSON representation of the changes made
    /// For Create: entire restaurant object
    /// For Update: fields that changed with old and new values
    /// For Delete: entire restaurant object before deletion
    /// </summary>
    public string? ChangeDetails { get; set; }
    
    /// <summary>
    /// Human-readable description of the changes
    /// </summary>
    public string? ChangeDescription { get; set; }
}


