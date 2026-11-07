CREATE VIEW [App].[site-read]
AS
SELECT t.[SiteId],
       t.[Name],
       t.[PrimaryHost],
       t.[DefaultCurrencyCode],
       t.[DefaultFulfillmentProviderCode],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Site] t

WHERE [AppDbo].[is-admin](0, 0) = 1 OR t.StatusCode = 'active';
