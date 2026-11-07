CREATE VIEW [App].[product-option-value-read]
AS
SELECT t.[ProductOptionValueId],
       r0.[ProductOptionId] AS [ProductOptionId],
       t.[ValueCode],
       t.[DisplayValue],
       t.[DisplaySequence],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[ProductOptionValue] t
LEFT JOIN [AppDbo].[ProductOption] r0 ON r0.RowId = t.[ProductOptionRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1 OR t.StatusCode = 'active';
