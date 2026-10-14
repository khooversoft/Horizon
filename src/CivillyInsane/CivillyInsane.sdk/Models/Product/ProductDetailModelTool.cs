using System.Collections.Frozen;
using Toolbox.Tools;
using Toolbox.Types;

namespace CivillyInsane.sdk.Models.Product;

public static class ProductDetailModelTool
{
    public static ProductDetailRecord ConvertTo(this ProductDetailModel model)
    {
        model.NotNull();

        var result = new ProductDetailRecord
        {
            ProductId = model.ProductId,
            ProductName = model.ProductName,
            Description = model.Description,
            ProductType = model.ProductType,
            Brand = model.Brand,
            CurrencyCode = model.CurrencyCode,
            Rating = model.Rating is null ? null : new ReviewDetail
            {
                Rating = model.Rating.Rating,
                Count = model.Rating.Count,
            },
            Prices = model.Prices.Select(price => new PriceDetail
            {
                Price = price.Price,
                Discounts = price.Discounts.Select(discount => new PriceDiscount
                {
                    Count = discount.Count,
                    DiscountPercentage = discount.DiscountPercentage,
                    DiscountPrice = discount.DiscountPrice,
                }).ToArray(),
                Fit = price.Fit.ToArray(),
                Style = price.Style.ToArray(),
                Size = price.Size.ToArray(),
                Colors = price.Colors.Select(color => new ColorDetail
                {
                    Name = color.Name,
                    Hex = color.Hex,
                }).ToArray(),
                Images = price.Images.ToArray(),
                Attributes = price.Attributes.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            }).ToArray(),
        };

        result.Validate().ThrowOnError();
        return result;
    }

    public static ProductDetailModel ConvertTo(this ProductDetailRecord record)
    {
        record.NotNull();

        return new ProductDetailModel
        {
            ProductId = record.ProductId,
            ProductName = record.ProductName,
            Description = record.Description,
            ProductType = record.ProductType,
            Brand = record.Brand,
            CurrencyCode = record.CurrencyCode,
            Rating = record.Rating is null ? null : new ReviewDetailModel
            {
                Rating = record.Rating.Rating,
                Count = record.Rating.Count,
            },
            Prices = record.Prices.Select(price => new PriceDetailModel
            {
                Price = price.Price,
                Discounts = price.Discounts.Select(discount => new PriceDiscountModel
                {
                    Count = discount.Count,
                    DiscountPercentage = discount.DiscountPercentage,
                    DiscountPrice = discount.DiscountPrice,
                }).ToList(),
                Fit = price.Fit.ToList(),
                Style = price.Style.ToList(),
                Size = price.Size.ToList(),
                Colors = price.Colors.Select(color => new ColorDetailModel
                {
                    Name = color.Name,
                    Hex = color.Hex,
                }).ToList(),
                Images = price.Images.ToList(),
                Attributes = new Dictionary<string, string>(price.Attributes, StringComparer.OrdinalIgnoreCase),
            }).ToList(),
        };
    }
}
