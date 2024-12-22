using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Coffee.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CartId { get; set; }
        
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}
