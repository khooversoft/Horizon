CREATE PROCEDURE [App].[product-option-value-get]
    @ProductOptionValueId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductOptionValueId], [ProductOptionId], [ValueCode], [DisplayValue], [DisplaySequence], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-option-value-read] WHERE [ProductOptionValueId] = @ProductOptionValueId;
END;
