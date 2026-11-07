CREATE VIEW [App].[sales-order-item-read]
AS
SELECT t.[SalesOrderItemId],
       r0.[SalesOrderId] AS [SalesOrderId],
       t.[ProductIdSnapshot],
       t.[ProductVariantIdSnapshot],
       t.[SkuSnapshot],
       t.[ProductNameSnapshot],
       t.[VariantNameSnapshot],
       t.[ProviderSkuSnapshot],
       t.[FulfillmentProviderCodeSnapshot],
       t.[Quantity],
       t.[UnitListPrice],
       t.[UnitSalePrice],
       t.[DiscountAmount],
       t.[TaxAmount],
       t.[LineTotal],
       t.CreatedUtc
FROM [AppDbo].[SalesOrderItem] t
LEFT JOIN [AppDbo].[SalesOrder] r0 ON r0.RowId = t.[SalesOrderRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
