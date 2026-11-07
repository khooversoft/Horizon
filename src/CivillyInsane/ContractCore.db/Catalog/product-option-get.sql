CREATE PROCEDURE [App].[product-option-get]
    @ProductOptionId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductOptionId], [Name], [DataTypeCode], [DisplaySequence], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-option-read] WHERE [ProductOptionId] = @ProductOptionId;
END;
