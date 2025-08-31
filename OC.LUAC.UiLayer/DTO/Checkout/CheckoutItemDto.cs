// UiLayer/DTO/Checkout/CheckoutItemDto.cs
namespace OC.LUAC.UiLayer.DTO.Checkout;

public class CheckoutItemDto
{
    public int ProductId { get; set; }

    // NEW: required by API
    public int ProductVariantId { get; set; }

    public string Size { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
