CREATE PROCEDURE [App].[variant-stock-adjust]
    @variant_id bigint,
    @delta int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE  [AppDbo].[variant]
    SET     [stock_qty] = [stock_qty] + @delta
            ,[modified_utc] = SYSUTCDATETIME()
    WHERE   [variant_id] = @variant_id
      AND   [stock_qty] + @delta >= 0;

    IF @@ROWCOUNT = 0 THROW 51002, 'conflict: variant not found or insufficient stock', 1;

    EXEC [App].[variant-get] @variant_id = @variant_id;
END;
