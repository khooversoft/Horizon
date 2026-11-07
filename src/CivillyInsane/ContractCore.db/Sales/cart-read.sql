CREATE VIEW [App].[cart-read]
AS
SELECT t.[CartId],
       r0.[SiteId] AS [SiteId],
       r1.[CustomerId] AS [CustomerId],
       t.[CurrencyCode],
       t.[StatusCode],
       t.[ExpiresUtc],
       t.[LastActivityUtc],
       r2.[SalesOrderId] AS [ConvertedOrderId],
       r3.[CartId] AS [MergedIntoCartId],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Cart] t
LEFT JOIN [AppDbo].[Site] r0 ON r0.RowId = t.[SiteRowId]
LEFT JOIN [AppDbo].[Customer] r1 ON r1.RowId = t.[CustomerRowId]
LEFT JOIN [AppDbo].[SalesOrder] r2 ON r2.RowId = t.[ConvertedOrderRowId]
LEFT JOIN [AppDbo].[Cart] r3 ON r3.RowId = t.[MergedIntoCartRowId]
WHERE [AppDbo].[can-read-cart](t.RowId) = 1;
