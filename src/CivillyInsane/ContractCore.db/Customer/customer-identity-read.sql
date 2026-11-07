CREATE VIEW [App].[customer-identity-read]
AS
SELECT t.[CustomerIdentityId],
       r0.[CustomerId] AS [CustomerId],
       t.[IdentityProviderCode],
       t.[LastSeenUtc],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[CustomerIdentity] t
LEFT JOIN [AppDbo].[Customer] r0 ON r0.RowId = t.[CustomerRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1;
