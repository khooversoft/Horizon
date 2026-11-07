CREATE VIEW [App].[subscription-item-read]
AS
SELECT t.[SubscriptionItemId],
       r0.[SubscriptionId] AS [SubscriptionId],
       r1.[ProductVariantId] AS [ProductVariantId],
       t.[Quantity],
       t.[PriceCeiling],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[SubscriptionItem] t
LEFT JOIN [AppDbo].[Subscription] r0 ON r0.RowId = t.[SubscriptionRowId]
LEFT JOIN [AppDbo].[ProductVariant] r1 ON r1.RowId = t.[ProductVariantRowId]
WHERE EXISTS (SELECT 1 FROM AppDbo.Subscription s WHERE s.RowId = t.SubscriptionRowId AND [AppDbo].[can-read-customer](s.CustomerRowId) = 1);
