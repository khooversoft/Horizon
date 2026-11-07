CREATE VIEW [App].[subscription-pet-read]
AS
SELECT t.[SubscriptionPetId],
       r0.[SubscriptionId] AS [SubscriptionId],
       r1.[PetId] AS [PetId],
       r2.[CustomerId] AS [CustomerId],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[SubscriptionPet] t
LEFT JOIN [AppDbo].[Subscription] r0 ON r0.RowId = t.[SubscriptionRowId]
LEFT JOIN [AppDbo].[Pet] r1 ON r1.RowId = t.[PetRowId]
LEFT JOIN [AppDbo].[Customer] r2 ON r2.RowId = t.[CustomerRowId]
WHERE [AppDbo].[can-read-customer](t.CustomerRowId) = 1;
