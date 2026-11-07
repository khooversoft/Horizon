CREATE VIEW [App].[product-option-read]
AS
SELECT t.[ProductOptionId],
       t.[Name],
       t.[DataTypeCode],
       t.[DisplaySequence],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[ProductOption] t

WHERE [AppDbo].[is-admin](0, 0) = 1 OR t.StatusCode = 'active';
