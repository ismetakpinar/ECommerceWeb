namespace ECommerce.Web.ViewModels.Order;

public class OrderListViewModel
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public int Status { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }


    public string StatusText =>
        Status switch
        {
            1 => "Bekliyor",
            2 => "Hazırlanıyor",
            3 => "Kargoda",
            4 => "Teslim Edildi",
            5 => "İptal Edildi",
            _ => "Bilinmiyor"
        };
}