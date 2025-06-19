using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models;

public class InboxMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;

    [Required]
    public DateTime ReceivedOnUtc { get; set; } = DateTime.UtcNow;

    public bool Processed { get; set; } = false;

    public DateTime? ProcessedOnUtc { get; set; }
}
