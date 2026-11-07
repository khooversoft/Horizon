CREATE VIEW [App].[subscription-read]
AS
SELECT t.[SubscriptionId],
       r0.[SiteId] AS [SiteId],
       r1.[CustomerId] AS [CustomerId],
       r2.[CustomerAddressId] AS [ShippingAddressId],
       r3.[PaymentMethodId] AS [PaymentMethodId],
       t.[FrequencyCode],
       t.[IntervalCount],
       t.[IntervalUnitCode],
       t.[StatusCode],
       t.[NextProcessUtc],
       t.[LastProcessUtc],
       t.[FailureCount],
       t.[DiscountPercent],
       t.[RetryAfterUtc],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Subscription] t
LEFT JOIN [AppDbo].[Site] r0 ON r0.RowId = t.[SiteRowId]
LEFT JOIN [AppDbo].[Customer] r1 ON r1.RowId = t.[CustomerRowId]
LEFT JOIN [AppDbo].[CustomerAddress] r2 ON r2.RowId = t.[ShippingAddressRowId]
LEFT JOIN [AppDbo].[PaymentMethod] r3 ON r3.RowId = t.[PaymentMethodRowId]
WHERE [AppDbo].[can-read-customer](t.CustomerRowId) = 1;
