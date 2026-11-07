CREATE VIEW [App].[sales-order-status-history-read]
AS
SELECT t.[SalesOrderStatusHistoryId],
       r0.[SalesOrderId] AS [SalesOrderId],
       t.[FromStatus],
       t.[ToStatus],
       t.[Reason],
       t.[Actor],
       t.[CorrelationId],
       t.[ChangedUtc],
       t.CreatedUtc
FROM [AppDbo].[SalesOrderStatusHistory] t
LEFT JOIN [AppDbo].[SalesOrder] r0 ON r0.RowId = t.[SalesOrderRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
