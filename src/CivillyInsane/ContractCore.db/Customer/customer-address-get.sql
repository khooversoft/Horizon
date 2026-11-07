CREATE PROCEDURE [App].[customer-address-get]
    @CustomerAddressId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CustomerAddressId], [CustomerId], [Label], [RecipientName], [CompanyName], [AddressLine1], [AddressLine2], [City], [RegionCode], [PostalCode], [CountryCode], [Phone], [IsDefaultShipping], [IsDefaultBilling], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[customer-address-read] WHERE [CustomerAddressId] = @CustomerAddressId;
END;
