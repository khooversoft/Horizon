CREATE PROCEDURE [App].[cart-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [CartId], [SiteId], [CustomerId], [CurrencyCode], [StatusCode], [ExpiresUtc], [LastActivityUtc], [ConvertedOrderId], [MergedIntoCartId], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[cart-read]
    ORDER BY [CartId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
