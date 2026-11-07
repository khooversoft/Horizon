CREATE PROCEDURE [App].[sales-order-address-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [SalesOrderAddressId], [SalesOrderId], [AddressTypeCode], [RecipientName], [CompanyName], [AddressLine1], [AddressLine2], [City], [RegionCode], [PostalCode], [CountryCode], [Phone], CreatedUtc FROM [App].[sales-order-address-read]
    ORDER BY [SalesOrderAddressId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
