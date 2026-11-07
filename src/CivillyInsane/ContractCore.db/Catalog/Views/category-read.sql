CREATE VIEW [App].[category-read]
AS
SELECT t.[CategoryId],
       r0.[CategoryId] AS [ParentCategoryId],
       t.[Name],
       t.[Slug],
       t.[StatusCode],
       t.[DisplaySequence],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Category] t
LEFT JOIN [AppDbo].[Category] r0 ON r0.RowId = t.[ParentCategoryRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR t.StatusCode = 'active';
