CREATE VIEW [App].[payment-read]
AS
SELECT t.[PaymentId],
       r0.[SalesOrderId] AS [SalesOrderId],
       t.[ProviderCode],
       t.[ProviderPaymentId],
       t.[StatusCode],
       t.[AmountRequested],
       t.[AmountAuthorized],
       t.[AmountCaptured],
       t.[AmountRefunded],
       t.[CurrencyCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Payment] t
LEFT JOIN [AppDbo].[SalesOrder] r0 ON r0.RowId = t.[SalesOrderRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
