using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace CivillyInsane.sdk.Services;

public class ShoppingCartService
{
    private readonly ILogger<ShoppingCartService> _logger;
    private readonly ConcurrentDictionary<string, ShippingCartItem> _cartItems = new();

    public ShoppingCartService(ILogger<ShoppingCartService> logger)
    {
        _logger = logger.NotNull();
    }

    public void AddToCart(string productId, int quantity)
    {
        _logger.LogInformation("Adding product {ProductId} with quantity {Quantity} to cart.", productId, quantity);

        _cartItems.AddOrUpdate(productId, new ShippingCartItem(productId, quantity), (key, existingItem) =>
        {
            return existingItem with { Quantity = existingItem.Quantity + quantity };
        });
    }

    public void RemoveFromCart(string productId)
    {
        _logger.LogInformation("Removing product {ProductId} from cart.", productId);
        _cartItems.TryRemove(productId, out _);
    }

    public void ClearCart()
    {
        _logger.LogInformation("Clearing the shopping cart.");
        _cartItems.Clear();
    }
}
