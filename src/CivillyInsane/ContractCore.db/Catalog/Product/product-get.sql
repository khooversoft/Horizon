CREATE PROCEDURE [App].[product-get]
    @product_id bigint = NULL,
    @slug varchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @product_id IS NULL AND @slug IS NULL THROW 51000, 'invalid-argument: product_id or slug required', 1;

    SELECT  *
    FROM    [AppDbo].[product-read]
    WHERE   (@product_id IS NOT NULL AND [product_id] = @product_id)
       OR   (@product_id IS NULL AND [slug] = @slug);
END;
