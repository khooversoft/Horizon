CREATE PROCEDURE [App].[unit-of-measure-get]
    @UnitOfMeasureId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [UnitOfMeasureId], [UnitCode], [Name], [DimensionCode], [BaseUnitCode], [ConversionFactor], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[unit-of-measure-read] WHERE [UnitOfMeasureId] = @UnitOfMeasureId;
END;
