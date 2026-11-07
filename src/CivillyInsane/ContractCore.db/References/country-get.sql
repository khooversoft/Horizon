CREATE PROCEDURE [App].[country-get]
    @CountryId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CountryId], [CountryCode], [Name], [IsSupported], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[country-read] WHERE [CountryId] = @CountryId;
END;
