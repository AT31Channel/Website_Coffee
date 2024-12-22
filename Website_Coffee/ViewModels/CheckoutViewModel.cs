using System.ComponentModel.DataAnnotations;
using Website_Coffee.Models;

namespace Website_Coffee.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Required]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        public List<CartItem> CartItems { get; set; }
        public decimal Total { get; set; }
    }
}
