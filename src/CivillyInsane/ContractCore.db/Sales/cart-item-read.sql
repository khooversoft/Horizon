CREATE VIEW [App].[cart-item-read]
AS
SELECT t.[CartItemId],
       r0.[CartId] AS [CartId],
       r1.[ProductVariantId] AS [ProductVariantId],
       t.[Quantity],
       t.[UnitPriceEstimate],
       t.[AddedUtc],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[CartItem] t
LEFT JOIN [AppDbo].[Cart] r0 ON r0.RowId = t.[CartRowId]
LEFT JOIN [AppDbo].[ProductVariant] r1 ON r1.RowId = t.[ProductVariantRowId]
WHERE [AppDbo].[can-read-cart](t.CartRowId) = 1;
