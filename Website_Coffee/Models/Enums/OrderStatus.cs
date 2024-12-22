using System.ComponentModel.DataAnnotations;

namespace Website_Coffee.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xác nhận")]
        Pending,
        
        [Display(Name = "Đã xác nhận")]
        Confirmed,

        [Display(Name = "Đang giao")]
        Shipping,

        [Display(Name = "Đã giao")]
        Delivered,

        [Display(Name = "Đã hủy")]
        Cancelled
    }
}
