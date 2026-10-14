using Toolbox.Tools;

namespace CivillyInsane.sdk.Services;

public record ShippingCartItem
{
    public ShippingCartItem() { }

    public ShippingCartItem(string productId, int quantity)
    {
        ProductId = productId.NotEmpty();
        Quantity = quantity.Assert(x => x > 0, "Quantity must be greater than zero.");
    }

    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}