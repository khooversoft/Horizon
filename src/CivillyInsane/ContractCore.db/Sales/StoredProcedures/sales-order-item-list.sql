CREATE PROCEDURE [App].[sales-order-item-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [SalesOrderItemId], [SalesOrderId], [ProductIdSnapshot], [ProductVariantIdSnapshot], [SkuSnapshot], [ProductNameSnapshot], [VariantNameSnapshot], [ProviderSkuSnapshot], [FulfillmentProviderCodeSnapshot], [Quantity], [UnitListPrice], [UnitSalePrice], [DiscountAmount], [TaxAmount], [LineTotal], CreatedUtc FROM [App].[sales-order-item-read]
    ORDER BY [SalesOrderItemId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
