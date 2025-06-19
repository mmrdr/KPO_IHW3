using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class GetAccountRequest
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
