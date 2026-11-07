CREATE PROCEDURE [App].[customer-identity-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [CustomerIdentityId], [CustomerId], [IdentityProviderCode], [LastSeenUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[customer-identity-read]
    ORDER BY [CustomerIdentityId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
