CREATE VIEW [App].[product-variant-read]
AS
SELECT t.[ProductVariantId],
       r0.[ProductId] AS [ProductId],
       t.[Sku],
       t.[Name],
       t.[UnitPrice],
       t.[CurrencyCode],
       t.[WeightValue],
       t.[WeightUnitCode],
       t.[FulfillmentProviderCode],
       t.[ProviderSku],
       t.[StatusCode],
       t.[SubscriptionEligible],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[ProductVariant] t
LEFT JOIN [AppDbo].[Product] r0 ON r0.RowId = t.[ProductRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR (t.StatusCode = 'active' AND EXISTS (SELECT 1 FROM AppDbo.Product p JOIN AppDbo.SiteProduct sp ON sp.ProductRowId = p.RowId JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE p.RowId = t.ProductRowId AND p.StatusCode = 'active' AND sp.IsVisible = 1 AND s.StatusCode = 'active'));
