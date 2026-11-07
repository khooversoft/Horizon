CREATE VIEW [App].[customer-address-read]
AS
SELECT t.[CustomerAddressId],
       r0.[CustomerId] AS [CustomerId],
       t.[Label],
       t.[RecipientName],
       t.[CompanyName],
       t.[AddressLine1],
       t.[AddressLine2],
       t.[City],
       t.[RegionCode],
       t.[PostalCode],
       t.[CountryCode],
       t.[Phone],
       t.[IsDefaultShipping],
       t.[IsDefaultBilling],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[CustomerAddress] t
LEFT JOIN [AppDbo].[Customer] r0 ON r0.RowId = t.[CustomerRowId]
WHERE [AppDbo].[can-read-customer](t.CustomerRowId) = 1;
