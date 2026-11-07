CREATE PROCEDURE [App].[product-get]
    @ProductId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductId], [Name], [ShortDescription], [Description], [ProductTypeCode], [StatusCode], [SubscriptionEligible], [DefaultFulfillmentProviderCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-read] WHERE [ProductId] = @ProductId;
END;
