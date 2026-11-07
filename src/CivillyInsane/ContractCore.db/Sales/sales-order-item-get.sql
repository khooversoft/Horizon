CREATE PROCEDURE [App].[sales-order-item-get]
    @SalesOrderItemId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SalesOrderItemId], [SalesOrderId], [ProductIdSnapshot], [ProductVariantIdSnapshot], [SkuSnapshot], [ProductNameSnapshot], [VariantNameSnapshot], [ProviderSkuSnapshot], [FulfillmentProviderCodeSnapshot], [Quantity], [UnitListPrice], [UnitSalePrice], [DiscountAmount], [TaxAmount], [LineTotal], CreatedUtc FROM [App].[sales-order-item-read] WHERE [SalesOrderItemId] = @SalesOrderItemId;
END;
