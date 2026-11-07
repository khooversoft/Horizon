CREATE PROCEDURE [App].[category-get]
    @CategoryId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [CategoryId], [ParentCategoryId], [Name], [Slug], [StatusCode], [DisplaySequence], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[category-read] WHERE [CategoryId] = @CategoryId;
END;
