CREATE VIEW [App].[product-read]
AS
SELECT t.[ProductId],
       t.[Name],
       t.[ShortDescription],
       t.[Description],
       t.[ProductTypeCode],
       t.[StatusCode],
       t.[SubscriptionEligible],
       t.[DefaultFulfillmentProviderCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Product] t

WHERE [AppDbo].[is-admin](0, 0) = 1 OR (t.StatusCode = 'active' AND EXISTS (SELECT 1 FROM AppDbo.SiteProduct sp JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE sp.ProductRowId = t.RowId AND sp.IsVisible = 1 AND s.StatusCode = 'active'));
