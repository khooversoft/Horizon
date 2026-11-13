CREATE PROCEDURE [App].[order-status-set]
    @order_id bigint,
    @status varchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @current varchar(20) = (SELECT [status] FROM [AppDbo].[order] WITH (UPDLOCK) WHERE [order_id] = @order_id);

    IF @current IS NULL THROW 51001, 'not-found: order', 1;

    IF @status = 'cancelled' AND @current NOT IN ('pending','paid') THROW 51002, 'conflict: order cannot be cancelled', 1;

    IF @status = 'cancelled'
    BEGIN
        UPDATE  v
        SET     [stock_qty] = v.[stock_qty] + i.[quantity]
                ,[modified_utc] = SYSUTCDATETIME()
        FROM    [AppDbo].[variant] v
        JOIN    (SELECT [variant_id], SUM([quantity]) AS [quantity] FROM [AppDbo].[order_item] WHERE [order_id] = @order_id GROUP BY [variant_id]) i
                ON i.[variant_id] = v.[variant_id];
    END;

    UPDATE  [AppDbo].[order]
    SET     [status] = @status
            ,[placed_utc] = CASE WHEN @status = 'paid' AND [placed_utc] IS NULL THEN SYSUTCDATETIME() ELSE [placed_utc] END
            ,[modified_utc] = SYSUTCDATETIME()
    WHERE   [order_id] = @order_id;

    COMMIT TRANSACTION;

    EXEC [App].[order-get] @order_id = @order_id;
END;
