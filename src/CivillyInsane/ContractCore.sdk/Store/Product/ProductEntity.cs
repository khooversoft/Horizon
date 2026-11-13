using System.Data;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public class ProductEntity
{
    private readonly ISqlClient _client;
    private readonly ILogger<ProductEntity> _logger;
    private readonly IAuthAccess? _authAccess;
    private readonly IStoreNotify? _storeNotify;

    public ProductEntity(ISqlClient client, ILogger<ProductEntity> logger, IAuthAccess? authAccess = null, IStoreNotify? storeNotify = null)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        _authAccess = authAccess;
        _storeNotify = storeNotify;
    }

    public async Task<Option<ProductRecord>> Save(ProductRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[product-save]", CommandType.StoredProcedure)
                .AddParameter("@product_id", record.ProductId)
                .AddParameter("@slug", record.Slug)
                .AddParameter("@name", record.Name)
                .AddParameter("@description", record.Description)
                .AddParameter("@base_price_cents", record.BasePriceCents)
                .AddParameter("@currency_code", record.CurrencyCode)
                .AddParameter("@status", record.Status)
                .AddParameter("@JsonData", record.JsonData)
                .Execute<ProductRecord>();

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Saved product {record.Slug}", $"Failed to save product {record.Slug}");
        return result;
    }

    public Task<Option<ProductRecord>> Get(long productId) => Get(productId, null);

    public Task<Option<ProductRecord>> Get(string slug) => Get(null, slug.NotEmpty());

    private Task<Option<ProductRecord>> Get(long? productId, string? slug) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[product-get]", CommandType.StoredProcedure)
            .AddParameter("@product_id", productId)
            .AddParameter("@slug", slug)
            .Execute<ProductRecord>();

        return list.ToSingle();
    });

    public Task<Option<IReadOnlyList<ProductRecord>>> List(string? status = null, int offset = 0, int pageSize = 100) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[product-list]", CommandType.StoredProcedure)
            .AddParameter("@status", status)
            .AddParameter("@Offset", offset)
            .AddParameter("@PageSize", pageSize)
            .Execute<ProductRecord>();

        return list.ToListOption();
    });
}
