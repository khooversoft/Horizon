CREATE PROCEDURE [App].[currency-get]
    @CurrencyId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CurrencyId], [CurrencyCode], [Name], [DecimalPlaces], [IsSupported], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[currency-read] WHERE [CurrencyId] = @CurrencyId;
END;
