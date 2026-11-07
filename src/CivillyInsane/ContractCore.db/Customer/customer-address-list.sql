CREATE PROCEDURE [App].[customer-address-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [CustomerAddressId], [CustomerId], [Label], [RecipientName], [CompanyName], [AddressLine1], [AddressLine2], [City], [RegionCode], [PostalCode], [CountryCode], [Phone], [IsDefaultShipping], [IsDefaultBilling], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[customer-address-read]
    ORDER BY [CustomerAddressId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
