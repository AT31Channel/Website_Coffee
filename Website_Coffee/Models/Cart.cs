using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Coffee.Models
{
    public class Cart
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
