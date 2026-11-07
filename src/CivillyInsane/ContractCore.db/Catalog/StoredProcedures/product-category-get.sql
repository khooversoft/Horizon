CREATE PROCEDURE [App].[product-category-get]
    @ProductCategoryId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductCategoryId], [ProductId], [CategoryId], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-category-read] WHERE [ProductCategoryId] = @ProductCategoryId;
END;
