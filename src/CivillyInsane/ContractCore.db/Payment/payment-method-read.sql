CREATE VIEW [App].[payment-method-read]
AS
SELECT t.[PaymentMethodId],
       r0.[CustomerId] AS [CustomerId],
       t.[ProviderCode],
       t.[MethodTypeCode],
       t.[DisplayBrand],
       t.[DisplayLastFour],
       t.[ExpirationMonth],
       t.[ExpirationYear],
       t.[IsDefault],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[PaymentMethod] t
LEFT JOIN [AppDbo].[Customer] r0 ON r0.RowId = t.[CustomerRowId]
WHERE [AppDbo].[can-read-customer](t.CustomerRowId) = 1;
