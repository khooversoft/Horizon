CREATE VIEW [App].[product-category-read]
AS
SELECT t.[ProductCategoryId],
       r0.[ProductId] AS [ProductId],
       r1.[CategoryId] AS [CategoryId],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[ProductCategory] t
LEFT JOIN [AppDbo].[Product] r0 ON r0.RowId = t.[ProductRowId]
LEFT JOIN [AppDbo].[Category] r1 ON r1.RowId = t.[CategoryRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR (1=1 AND EXISTS (SELECT 1 FROM AppDbo.Product p JOIN AppDbo.SiteProduct sp ON sp.ProductRowId = p.RowId JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE p.RowId = t.ProductRowId AND p.StatusCode = 'active' AND sp.IsVisible = 1 AND s.StatusCode = 'active'));
