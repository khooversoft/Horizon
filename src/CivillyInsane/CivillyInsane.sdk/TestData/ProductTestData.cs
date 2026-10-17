using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CivillyInsane.sdk.Actors;
using CivillyInsane.sdk.Models.Product;
using Microsoft.Extensions.FileProviders;
using Toolbox.Tools;
using Toolbox.Types;

namespace CivillyInsane.sdk.TestData;

public static class ProductTestData
{
    // Scan wwwroot/images for supported image files and create one ProductDetailRecord per image.
    // Use the filename without its extension or leading numeric prefix (such as "01_") as the ProductId.
    // Form the ProductName by replacing dashes with spaces and capitalizing the first character.
    // Set the Description to "A tee shirt for when you feel {productName}."
    // Set ProductType to "TeeShirt", Brand to "Civilly Insane", and CurrencyCode to "USD".
    // Add one PriceDetail with a random price from $30.00 through $40.00, inclusive.
    // Include the configured fit, style, and size options in the price detail.
    // Apply quantity discounts of 10%, 20%, 30%, and 40% for 10, 20, 30, and 40 items, respectively.
    // Upload each image to the actor's file store under its ProductId and original filename.
    // Reference the stored image by its filename in the price detail's Images collection.
    // Upsert the product record and check both image and product upsert results for success.
    //
    public static async Task UpsertAll(ProductStoreActor actor, IFileProvider webRootFileProvider, bool force = false)
    {
        actor.NotNull();
        webRootFileProvider.NotNull();

        IReadOnlyList<string> imageNames = GetAllImages(webRootFileProvider);
        foreach (var imageName in imageNames)
        {
            var productId = Path.GetFileNameWithoutExtension(imageName);
            productId = Regex.Replace(productId, @"^[0-9]+_", string.Empty);
            var productName = productId.Replace('-', ' ').Replace('_', ' ');
            productName = char.ToUpperInvariant(productName[0]) + productName[1..];

            if (!force && (await actor.Exists(productId, imageName)).IsOk()) continue;

            await LoadImage(actor, productId, $"images/{imageName}", webRootFileProvider);
            var product = CreateProduct(productId, productName, [imageName]);
            (await actor.Upsert(product)).BeOk();
        }
    }

    private static ProductDetailRecord CreateProduct(string productId, string productName, IEnumerable<string> imageNames) => new()
    {
        ProductId = productId,
        ProductName = productName,
        Description = $"A tee shirt for when you feel {productName}.",
        ProductType = nameof(ProductType.TeeShirt),
        Brand = "Civilly Insane",
        CurrencyCode = "USD",
        Rating = new ReviewDetail { Rating = Random.Shared.Next(1, 6), Count = Random.Shared.Next(0, 1001) },
        Prices =
        [
            new PriceDetail
            {
                Price = Random.Shared.Next(3000, 4001) / 100m,
                Fit = ["Male Fit", "Female Fit"],
                Style = ["Classic", "Heavyweight", "Extra-Soft", "Streewear", "Premium", "V-Neck", "Vintage", "Active"],
                Size = ["S", "M", "L", "XL", "2XL", "3XL", "4XL", "5XL"],

                Images = imageNames.ToArray(),

                Discounts =
                [
                    new PriceDiscount { Count = 10, DiscountPercentage = 10 },
                    new PriceDiscount { Count = 20, DiscountPercentage = 20 },
                    new PriceDiscount { Count = 30, DiscountPercentage = 30 },
                    new PriceDiscount { Count = 40, DiscountPercentage = 40 },
                ],
            },
        ],
    };

    private static async Task LoadImage(ProductStoreActor actor, string productId, string imagePath, IFileProvider webRootFileProvider)
    {
        webRootFileProvider.NotNull();

        var imageFile = webRootFileProvider.GetFileInfo(imagePath);
        if (!imageFile.Exists) throw new FileNotFoundException($"Image file not found in wwwroot: {imagePath}", imagePath);

        using var imageStream = imageFile.CreateReadStream();
        using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer);
        var imageBytes = buffer.ToArray();
        var imageName = imageFile.Name;

        // Load local relative image into DataETag
        var data = imageBytes.ToDataETag();

        // Upload the image to the actor's file store
        (await actor.Upsert(productId, imageName, data)).BeOk();
    }

    private static IReadOnlyList<string> GetAllImages(IFileProvider webRootFileProvider)
    {
        var list = webRootFileProvider.GetDirectoryContents("images")
            .Where(x => !x.IsDirectory && Path.GetExtension(x.Name).ToLowerInvariant() is
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".svg" or ".bmp" or ".avif" or ".ico" or ".tif" or ".tiff")
            .Select(x => x.Name)
            .ToImmutableArray();

        return list;
    }
}