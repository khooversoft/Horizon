CREATE VIEW [App].[product-variant-option-value-read]
AS
SELECT t.[ProductVariantOptionValueId],
       r0.[ProductVariantId] AS [ProductVariantId],
       r1.[ProductOptionId] AS [ProductOptionId],
       r2.[ProductOptionValueId] AS [ProductOptionValueId],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[ProductVariantOptionValue] t
LEFT JOIN [AppDbo].[ProductVariant] r0 ON r0.RowId = t.[ProductVariantRowId]
LEFT JOIN [AppDbo].[ProductOption] r1 ON r1.RowId = t.[ProductOptionRowId]
LEFT JOIN [AppDbo].[ProductOptionValue] r2 ON r2.RowId = t.[ProductOptionValueRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR EXISTS (SELECT 1 FROM AppDbo.ProductVariant v JOIN AppDbo.Product p ON p.RowId = v.ProductRowId JOIN AppDbo.SiteProduct sp ON sp.ProductRowId = p.RowId JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE v.RowId = t.ProductVariantRowId AND v.StatusCode = 'active' AND p.StatusCode = 'active' AND sp.IsVisible = 1 AND s.StatusCode = 'active');
