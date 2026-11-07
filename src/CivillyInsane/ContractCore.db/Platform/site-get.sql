CREATE PROCEDURE [App].[site-get]
    @SiteId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SiteId], [Name], [PrimaryHost], [DefaultCurrencyCode], [DefaultFulfillmentProviderCode], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[site-read] WHERE [SiteId] = @SiteId;
END;
