using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Tools;

namespace ContractCore.sdk;

public class ContractCoreStore
{
    public ContractCoreStore(ISqlClient<ContractCoreStore> client, IServiceProvider serviceProvider)
    {
        client.NotNull();
        serviceProvider.NotNull();

        Products = ActivatorUtilities.CreateInstance<ProductEntity>(serviceProvider, client);
        Variants = ActivatorUtilities.CreateInstance<VariantEntity>(serviceProvider, client);
        Customers = ActivatorUtilities.CreateInstance<CustomerEntity>(serviceProvider, client);
        Orders = ActivatorUtilities.CreateInstance<OrderEntity>(serviceProvider, client);
        Payments = ActivatorUtilities.CreateInstance<PaymentEntity>(serviceProvider, client);
        Shipments = ActivatorUtilities.CreateInstance<ShipmentEntity>(serviceProvider, client);
    }

    public ProductEntity Products { get; }
    public VariantEntity Variants { get; }
    public CustomerEntity Customers { get; }
    public OrderEntity Orders { get; }
    public PaymentEntity Payments { get; }
    public ShipmentEntity Shipments { get; }
}
