CREATE VIEW [App].[sales-order-discount-read]
AS
SELECT t.[SalesOrderDiscountId],
       r0.[SalesOrderId] AS [SalesOrderId],
       t.[Code],
       t.[Description],
       t.[DiscountTypeCode],
       t.[Amount],
       t.CreatedUtc
FROM [AppDbo].[SalesOrderDiscount] t
LEFT JOIN [AppDbo].[SalesOrder] r0 ON r0.RowId = t.[SalesOrderRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
