CREATE PROCEDURE [App].[subscription-item-get]
    @SubscriptionItemId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SubscriptionItemId], [SubscriptionId], [ProductVariantId], [Quantity], [PriceCeiling], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[subscription-item-read] WHERE [SubscriptionItemId] = @SubscriptionItemId;
END;
