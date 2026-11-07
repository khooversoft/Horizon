CREATE PROCEDURE [App].[customer-get]
    @CustomerId nvarchar(320)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CustomerId], [EmailDisplay], [FirstName], [LastName], [Phone], [StatusCode], [MarketingOptIn], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[customer-read] WHERE [CustomerId] = @CustomerId;
END;
