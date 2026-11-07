CREATE VIEW [App].[payment-transaction-read]
AS
SELECT t.[PaymentTransactionId],
       r0.[PaymentId] AS [PaymentId],
       t.[ProviderCode],
       t.[TransactionTypeCode],
       t.[ProviderTransactionId],
       t.[ProviderEventId],
       t.[Amount],
       t.[CurrencyCode],
       t.[StatusCode],
       t.[FailureCode],
       t.[OccurredUtc],
       t.CreatedUtc
FROM [AppDbo].[PaymentTransaction] t
LEFT JOIN [AppDbo].[Payment] r0 ON r0.RowId = t.[PaymentRowId]
WHERE EXISTS (SELECT 1 FROM AppDbo.Payment p WHERE p.RowId = t.PaymentRowId AND [AppDbo].[can-read-order](p.SalesOrderRowId) = 1);
