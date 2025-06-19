using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models
{
    public class DepositRequest
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}
