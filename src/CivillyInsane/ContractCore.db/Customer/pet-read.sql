CREATE VIEW [App].[pet-read]
AS
SELECT t.[PetId],
       r0.[CustomerId] AS [CustomerId],
       t.[Name],
       t.[SpeciesCode],
       t.[BirthDate],
       t.[Breed],
       t.[Weight],
       t.[StatusCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Pet] t
LEFT JOIN [AppDbo].[Customer] r0 ON r0.RowId = t.[CustomerRowId]
WHERE [AppDbo].[can-read-customer](t.CustomerRowId) = 1;
