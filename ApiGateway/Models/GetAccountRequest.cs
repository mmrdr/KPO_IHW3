using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models
{
    public class GetAccountRequest
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
