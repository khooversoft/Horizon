CREATE VIEW [App].[unit-of-measure-read]
AS
SELECT t.[UnitOfMeasureId],
       t.[UnitCode],
       t.[Name],
       t.[DimensionCode],
       t.[BaseUnitCode],
       t.[ConversionFactor],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[UnitOfMeasure] t

WHERE 1 = 1;
