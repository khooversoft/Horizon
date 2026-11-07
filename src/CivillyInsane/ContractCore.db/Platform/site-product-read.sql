CREATE VIEW [App].[site-product-read]
AS
SELECT t.[SiteProductId],
       r0.[SiteId] AS [SiteId],
       r1.[ProductId] AS [ProductId],
       t.[IsVisible],
       t.[DisplayNameOverride],
       t.[DisplaySequence],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[SiteProduct] t
LEFT JOIN [AppDbo].[Site] r0 ON r0.RowId = t.[SiteRowId]
LEFT JOIN [AppDbo].[Product] r1 ON r1.RowId = t.[ProductRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR (t.IsVisible = 1 AND EXISTS (SELECT 1 FROM AppDbo.Site s WHERE s.RowId = t.SiteRowId AND s.StatusCode = 'active') AND EXISTS (SELECT 1 FROM AppDbo.Product p WHERE p.RowId = t.ProductRowId AND p.StatusCode = 'active'));
