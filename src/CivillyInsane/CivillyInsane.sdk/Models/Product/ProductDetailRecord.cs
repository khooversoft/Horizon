using System.Collections.Frozen;
using Toolbox.Tools;
using Toolbox.Types;

namespace CivillyInsane.sdk.Models.Product;

public record ProductDetailRecord
{
    public string ProductId { get; init; } = null!;
    public string ProductName { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string ProductType { get; init; } = null!;
    public string Brand { get; init; } = null!;
    public string CurrencyCode { get; init; } = null!;
    public ReviewDetail? Rating { get; init; }

    public IReadOnlyList<PriceDetail> Prices { get; init; } = Array.Empty<PriceDetail>();

    public static IValidator<ProductDetailRecord> Validator { get; } = new Validator<ProductDetailRecord>()
        .RuleFor(x => x.ProductId).NotEmpty()
        .RuleFor(x => x.ProductName).NotEmpty()
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.ProductType).NotEmpty()
        .RuleFor(x => x.Brand).NotEmpty()
        .RuleFor(x => x.CurrencyCode).NotEmpty()
        .RuleFor(x => x.Rating).ValidateOption(ReviewDetail.Validator)
        .Build();
}

public static class ProductDetailRecordTool
{
    public static Option Validate(this ProductDetailRecord subject) => ProductDetailRecord.Validator.Validate(subject).ToOptionStatus();
    public static decimal GetFirstPrice(this ProductDetailRecord subject) => subject.Prices.FirstOrDefault()?.Price ?? 0.0m;
}

/// <summary>
/// Fit, Style, and Size are selectable options for the product. For example, a t-shirt may have different fits 
/// (e.g., slim, regular, relaxed), styles (e.g., v-neck, crew neck), and sizes (e.g., S, M, L, XL). 
/// Each of these options can have different prices and discounts associated with them.
/// </summary>
public record PriceDetail
{
    public decimal Price { get; init; }
    public IReadOnlyList<PriceDiscount> Discounts { get; init; } = Array.Empty<PriceDiscount>();
    public IReadOnlyList<string> Fit { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Style { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Size { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ColorDetail> Colors { get; init; } = Array.Empty<ColorDetail>();

    // Image paths in datalake, relative to the product folder. Example: ["images/product1/image1.jpg", "images/product1/image2.jpg"]
    public IReadOnlyList<string> Images { get; init; } = Array.Empty<string>();

    // Key value attributes, see "ProductAttribute"
    public IReadOnlyDictionary<string, string> Attributes { get; init; } = FrozenDictionary<string, string>.Empty;

    public static IValidator<PriceDetail> Validator { get; } = new Validator<PriceDetail>()
        .RuleFor(x => x.Price).Must(x => x >= 0.0m, _ => "Price must be greater than or equal to zero")
        .RuleFor(x => x.Discounts).NotNull()
        .RuleForEach(x => x.Discounts).Validate(PriceDiscount.Validator)
        .RuleFor(x => x.Fit).NotNull()
        .RuleFor(x => x.Style).NotNull()
        .RuleFor(x => x.Size).NotNull()
        .RuleFor(x => x.Colors).NotNull()
        .RuleForEach(x => x.Colors).Validate(ColorDetail.Validator)
        .RuleFor(x => x.Images).NotNull()
        .RuleFor(x => x.Attributes).NotNull()
        .Build();
}

public record PriceDiscount
{
    // Count or greater than this count will get the discount price
    public int Count { get; init; }
    public double? DiscountPercentage { get; init; }
    public decimal? DiscountPrice { get; init; }

    public static IValidator<PriceDiscount> Validator { get; } = new Validator<PriceDiscount>()
        .RuleFor(x => x.Count).Must(x => x >= 0, _ => "Count must be greater than or equal to zero")
        .RuleFor(x => x.DiscountPercentage).Must(x => x is null or (>= 0.0 and <= 100.0), _ => "DiscountPercentage must be between 0 and 100")
        .RuleFor(x => x.DiscountPrice).Must(x => x is null or >= 0.0m, _ => "DiscountPrice must be greater than or equal to zero")
        .RuleForObject(x => x).Must(x => x.DiscountPercentage is not null || x.DiscountPrice is not null, _ => "Either DiscountPercentage or DiscountPrice must be provided")
        .Build();
}

public record ReviewDetail
{
    public decimal Rating { get; init; }
    public int Count { get; init; }

    public static IValidator<ReviewDetail> Validator { get; } = new Validator<ReviewDetail>()
        .RuleFor(x => x.Rating).Must(x => x >= 0.0m && x <= 5.0m, _ => "Rating must be between 0 and 5")
        .RuleFor(x => x.Count).Must(x => x >= 0, _ => "Count must be greater than or equal to zero")
        .Build();
}

public record ColorDetail
{
    public string Name { get; init; } = null!;
    public string Hex { get; init; } = null!;

    public static IValidator<ColorDetail> Validator { get; } = new Validator<ColorDetail>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Hex).NotEmpty()
        .Build();
}
