CREATE VIEW [App].[sales-order-address-read]
AS
SELECT t.[SalesOrderAddressId],
       r0.[SalesOrderId] AS [SalesOrderId],
       t.[AddressTypeCode],
       t.[RecipientName],
       t.[CompanyName],
       t.[AddressLine1],
       t.[AddressLine2],
       t.[City],
       t.[RegionCode],
       t.[PostalCode],
       t.[CountryCode],
       t.[Phone],
       t.CreatedUtc
FROM [AppDbo].[SalesOrderAddress] t
LEFT JOIN [AppDbo].[SalesOrder] r0 ON r0.RowId = t.[SalesOrderRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
