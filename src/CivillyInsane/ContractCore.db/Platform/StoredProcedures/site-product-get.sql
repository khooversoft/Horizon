CREATE PROCEDURE [App].[site-product-get]
    @SiteProductId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SiteProductId], [SiteId], [ProductId], [IsVisible], [DisplayNameOverride], [DisplaySequence], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[site-product-read] WHERE [SiteProductId] = @SiteProductId;
END;
