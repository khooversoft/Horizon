CREATE PROCEDURE [App].[cart-item-get]
    @CartItemId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CartItemId], [CartId], [ProductVariantId], [Quantity], [UnitPriceEstimate], [AddedUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[cart-item-read] WHERE [CartItemId] = @CartItemId;
END;
