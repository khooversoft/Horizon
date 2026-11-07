CREATE PROCEDURE [App].[product-variant-get]
    @ProductVariantId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductVariantId], [ProductId], [Sku], [Name], [UnitPrice], [CurrencyCode], [WeightValue], [WeightUnitCode], [FulfillmentProviderCode], [ProviderSku], [StatusCode], [SubscriptionEligible], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-variant-read] WHERE [ProductVariantId] = @ProductVariantId;
END;
