using System;
using System.Collections.Generic;
using System.Text;
using CivillyInsane.sdk.Actors;
using CivillyInsane.sdk.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace CivillyInsane.sdk.Services;

// Add API method for retrieving from the datalake store
// The API path is /api/images/{productId}/{imageId}

public class ImageApiService
{
    private readonly ILogger<ImageApiService> _logger;
    private readonly ProductStoreActor _actor;

    public ImageApiService(ProductStoreActor actor, ILogger<ImageApiService> logger)
    {
        _logger = logger.NotNull();
        _actor = actor.NotNull();
    }

    // Function to register for asp.net minimal API
    public static void RegisterApi(IEndpointRouteBuilder endpoints)
    {
        var contentTypeProvider = new FileExtensionContentTypeProvider();

        endpoints.NotNull().MapGet("/api/images/{productId}/{imageId}", async Task<IResult> (string productId, string imageId, ImageApiService service) =>
        {
            if (productId is "." or ".." || imageId is "." or ".." ||
                productId.Contains('/') || productId.Contains('\\') || imageId.Contains('/') || imageId.Contains('\\'))
            {
                service._logger.LogWarning("Rejected invalid image path, productId={productId}, imageId={imageId}", productId, imageId);
                return Results.BadRequest();
            }

            if (!contentTypeProvider.TryGetContentType(imageId, out var contentType) ||
                !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                service._logger.LogDebug("Rejected unsupported image type, productId={productId}, imageId={imageId}", productId, imageId);
                return Results.NotFound();
            }

            var result = await service.GetImage(productId, imageId);
            if (!result.IsOk()) return Results.StatusCode((int)result.StatusCode.ToHttpStatusCode());

            return Results.File(result.Return().Data.ToArray(), contentType);
        })
        .AllowAnonymous();
    }

    public static string BuildApiPath(string productId, string imageId) => $"/api/images/{productId.NotEmpty()}/{imageId.NotEmpty()}";

    private async Task<Option<DataETag>> GetImage(string productId, string imageId)
    {
        _logger.LogDebug("Retrieving image, productId={productId}, imageId={imageId}", productId, imageId);

        try
        {
            var result = await _actor.Get(productId, imageId);
            if (result.IsOk())
            {
                _logger.LogDebug("Retrieved image, productId={productId}, imageId={imageId}, byteCount={byteCount}", productId, imageId, result.Return().Data.Length);
            }
            else
            {
                var logLevel = result.StatusCode.IsNotFound() || result.StatusCode.IsNoContent()
                    ? LogLevel.Debug
                    : (int)result.StatusCode.ToHttpStatusCode() >= 500 ? LogLevel.Error : LogLevel.Warning;

                _logger.Log(logLevel, "Failed to retrieve image, productId={productId}, imageId={imageId}, statusCode={statusCode}", productId, imageId, result.StatusCode);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception retrieving image, productId={productId}, imageId={imageId}", productId, imageId);
            return StatusCode.InternalServerError;
        }
    }
}
