CREATE PROCEDURE [App].[product-media-get]
    @ProductMediaId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [ProductMediaId], [ProductId], [ProductVariantId], [MediaTypeCode], [StorageUri], [ThumbnailUri], [AltText], [DisplaySequence], [IsPrimary], [ContentType], [Width], [Height], [DurationSeconds], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[product-media-read] WHERE [ProductMediaId] = @ProductMediaId;
END;
