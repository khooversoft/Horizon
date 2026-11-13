CREATE PROCEDURE [App].[variant-list]
    @product_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT  *
    FROM    [AppDbo].[variant-read]
    WHERE   [product_id] = @product_id
    ORDER BY [color], [size], [variant_id];
END;
