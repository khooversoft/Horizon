CREATE PROCEDURE [App].[cart-get]
    @CartId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CartId], [SiteId], [CustomerId], [CurrencyCode], [StatusCode], [ExpiresUtc], [LastActivityUtc], [ConvertedOrderId], [MergedIntoCartId], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[cart-read] WHERE [CartId] = @CartId;
END;
