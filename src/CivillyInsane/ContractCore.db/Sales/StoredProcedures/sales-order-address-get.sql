CREATE PROCEDURE [App].[sales-order-address-get]
    @SalesOrderAddressId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SalesOrderAddressId], [SalesOrderId], [AddressTypeCode], [RecipientName], [CompanyName], [AddressLine1], [AddressLine2], [City], [RegionCode], [PostalCode], [CountryCode], [Phone], CreatedUtc FROM [App].[sales-order-address-read] WHERE [SalesOrderAddressId] = @SalesOrderAddressId;
END;
