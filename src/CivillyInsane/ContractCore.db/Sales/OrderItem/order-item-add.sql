CREATE PROCEDURE [App].[order-item-add]
    @order_id bigint,
    @variant_id bigint,
    @quantity int,
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @quantity IS NULL OR @quantity <= 0 THROW 51000, 'invalid-argument: quantity', 1;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM [AppDbo].[order] WITH (UPDLOCK) WHERE [order_id] = @order_id AND [status] = 'pending')
        THROW 51001, 'not-found: pending order', 1;

    UPDATE  [AppDbo].[variant]
    SET     [stock_qty] = [stock_qty] - @quantity
            ,[modified_utc] = SYSUTCDATETIME()
    WHERE   [variant_id] = @variant_id
      AND   [status] = 'active'
      AND   [stock_qty] >= @quantity;

    IF @@ROWCOUNT = 0 THROW 51002, 'conflict: variant unavailable or insufficient stock', 1;

    INSERT INTO [AppDbo].[order_item] ([order_id], [variant_id], [sku], [product_name], [size], [color], [unit_price_cents], [quantity], [JsonData])
    SELECT  @order_id
            ,v.[variant_id]
            ,v.[sku]
            ,p.[name]
            ,v.[size]
            ,v.[color]
            ,COALESCE(v.[price_cents], p.[base_price_cents])
            ,@quantity
            ,@JsonData
    FROM    [AppDbo].[variant] v
    JOIN    [AppDbo].[product] p ON p.[product_id] = v.[product_id]
    WHERE   v.[variant_id] = @variant_id;

    UPDATE  o
    SET     [subtotal_cents] = (SELECT COALESCE(SUM([line_total_cents]), 0) FROM [AppDbo].[order_item] WHERE [order_id] = @order_id)
            ,[modified_utc] = SYSUTCDATETIME()
    FROM    [AppDbo].[order] o
    WHERE   o.[order_id] = @order_id;

    COMMIT TRANSACTION;

    EXEC [App].[order-get] @order_id = @order_id;
END;
