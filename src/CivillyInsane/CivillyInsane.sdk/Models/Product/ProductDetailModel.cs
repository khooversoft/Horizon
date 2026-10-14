namespace CivillyInsane.sdk.Models.Product;

public class ProductDetailModel
{
    public string ProductId { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ProductType { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;
    public ReviewDetailModel? Rating { get; set; }
    public List<PriceDetailModel> Prices { get; set; } = [];
}

public class PriceDetailModel
{
    public decimal Price { get; set; }
    public List<PriceDiscountModel> Discounts { get; set; } = [];
    public List<string> Fit { get; set; } = [];
    public List<string> Style { get; set; } = [];
    public List<string> Size { get; set; } = [];
    public List<ColorDetailModel> Colors { get; set; } = [];
    public List<string> Images { get; set; } = [];
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class PriceDiscountModel
{
    public int Count { get; set; }
    public double? DiscountPercentage { get; set; }
    public decimal? DiscountPrice { get; set; }
}

public class ReviewDetailModel
{
    public decimal Rating { get; set; }
    public int Count { get; set; }
}

public class ColorDetailModel
{
    public string Name { get; set; } = null!;
    public string Hex { get; set; } = null!;
}
