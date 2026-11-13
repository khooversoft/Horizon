using System.Data;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public class VariantEntity
{
    private readonly ISqlClient _client;
    private readonly ILogger<VariantEntity> _logger;
    private readonly IAuthAccess? _authAccess;
    private readonly IStoreNotify? _storeNotify;

    public VariantEntity(ISqlClient client, ILogger<VariantEntity> logger, IAuthAccess? authAccess = null, IStoreNotify? storeNotify = null)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        _authAccess = authAccess;
        _storeNotify = storeNotify;
    }

    public async Task<Option<VariantRecord>> Save(VariantRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[variant-save]", CommandType.StoredProcedure)
                .AddParameter("@variant_id", record.VariantId)
                .AddParameter("@product_id", record.ProductId)
                .AddParameter("@sku", record.Sku)
                .AddParameter("@size", record.Size)
                .AddParameter("@color", record.Color)
                .AddParameter("@price_cents", record.PriceCents)
                .AddParameter("@stock_qty", record.StockQty)
                .AddParameter("@status", record.Status)
                .AddParameter("@JsonData", record.JsonData)
                .Execute<VariantRecord>();

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Saved variant {record.Sku}", $"Failed to save variant {record.Sku}");
        return result;
    }

    public async Task<Option<VariantRecord>> AdjustStock(long variantId, int delta)
    {
        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[variant-stock-adjust]", CommandType.StoredProcedure)
                .AddParameter("@variant_id", variantId)
                .AddParameter("@delta", delta)
                .Execute<VariantRecord>();

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Adjusted stock for variant {variantId}", $"Failed to adjust stock for variant {variantId}");
        return result;
    }

    public Task<Option<VariantRecord>> Get(long variantId) => Get(variantId, null);

    public Task<Option<VariantRecord>> Get(string sku) => Get(null, sku.NotEmpty());

    private Task<Option<VariantRecord>> Get(long? variantId, string? sku) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[variant-get]", CommandType.StoredProcedure)
            .AddParameter("@variant_id", variantId)
            .AddParameter("@sku", sku)
            .Execute<VariantRecord>();

        return list.ToSingle();
    });

    public Task<Option<IReadOnlyList<VariantRecord>>> List(long productId) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[variant-list]", CommandType.StoredProcedure)
            .AddParameter("@product_id", productId)
            .Execute<VariantRecord>();

        return list.ToListOption();
    });
}
