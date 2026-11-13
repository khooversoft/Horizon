CREATE PROCEDURE [App].[variant-get]
    @variant_id bigint = NULL,
    @sku varchar(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @variant_id IS NULL AND @sku IS NULL THROW 51000, 'invalid-argument: variant_id or sku required', 1;

    SELECT  *
    FROM    [AppDbo].[variant-read]
    WHERE   (@variant_id IS NOT NULL AND [variant_id] = @variant_id)
       OR   (@variant_id IS NULL AND [sku] = @sku);
END;
