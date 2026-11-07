CREATE VIEW [App].[fulfillment-item-read]
AS
SELECT t.[FulfillmentItemId],
       r0.[FulfillmentId] AS [FulfillmentId],
       r1.[SalesOrderId] AS [SalesOrderId],
       r2.[SalesOrderItemId] AS [SalesOrderItemId],
       t.[Quantity],
       t.CreatedUtc
FROM [AppDbo].[FulfillmentItem] t
LEFT JOIN [AppDbo].[Fulfillment] r0 ON r0.RowId = t.[FulfillmentRowId]
LEFT JOIN [AppDbo].[SalesOrder] r1 ON r1.RowId = t.[SalesOrderRowId]
LEFT JOIN [AppDbo].[SalesOrderItem] r2 ON r2.RowId = t.[SalesOrderItemRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
