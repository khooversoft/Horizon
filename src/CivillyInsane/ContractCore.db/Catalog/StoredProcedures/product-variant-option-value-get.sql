CREATE PROCEDURE [App].[product-variant-option-value-get]
    @ProductVariantOptionValueId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductVariantOptionValueId], [ProductVariantId], [ProductOptionId], [ProductOptionValueId], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-variant-option-value-read] WHERE [ProductVariantOptionValueId] = @ProductVariantOptionValueId;
END;
