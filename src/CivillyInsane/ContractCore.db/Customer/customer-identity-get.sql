CREATE PROCEDURE [App].[customer-identity-get]
    @CustomerIdentityId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CustomerIdentityId], [CustomerId], [IdentityProviderCode], [LastSeenUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[customer-identity-read] WHERE [CustomerIdentityId] = @CustomerIdentityId;
END;
