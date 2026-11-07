CREATE VIEW [App].[product-media-read]
AS
SELECT t.[ProductMediaId],
       r0.[ProductId] AS [ProductId],
       r1.[ProductVariantId] AS [ProductVariantId],
       t.[MediaTypeCode],
       t.[StorageUri],
       t.[ThumbnailUri],
       t.[AltText],
       t.[DisplaySequence],
       t.[IsPrimary],
       t.[ContentType],
       t.[Width],
       t.[Height],
       t.[DurationSeconds],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[ProductMedia] t
LEFT JOIN [AppDbo].[Product] r0 ON r0.RowId = t.[ProductRowId]
LEFT JOIN [AppDbo].[ProductVariant] r1 ON r1.RowId = t.[ProductVariantRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR (t.StatusCode = 'active' AND EXISTS (SELECT 1 FROM AppDbo.Product p JOIN AppDbo.SiteProduct sp ON sp.ProductRowId = p.RowId JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE p.RowId = t.ProductRowId AND p.StatusCode = 'active' AND sp.IsVisible = 1 AND s.StatusCode = 'active'));
