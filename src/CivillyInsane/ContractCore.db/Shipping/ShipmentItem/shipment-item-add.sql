CREATE PROCEDURE [App].[shipment-item-add]
    @shipment_id bigint,
    @order_item_id bigint,
    @quantity int,
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @quantity IS NULL OR @quantity <= 0 THROW 51000, 'invalid-argument: quantity', 1;

    BEGIN TRANSACTION;

    DECLARE @ordered int, @shipped int;

    SELECT  @ordered = oi.[quantity]
    FROM    [AppDbo].[order_item] oi WITH (UPDLOCK)
    JOIN    [AppDbo].[shipment] s ON s.[order_id] = oi.[order_id]
    WHERE   oi.[order_item_id] = @order_item_id
      AND   s.[shipment_id] = @shipment_id;

    IF @ordered IS NULL THROW 51001, 'not-found: order item for shipment order', 1;

    SELECT  @shipped = COALESCE(SUM([quantity]), 0)
    FROM    [AppDbo].[shipment_item]
    WHERE   [order_item_id] = @order_item_id;

    IF @shipped + @quantity > @ordered THROW 51002, 'conflict: quantity exceeds unshipped quantity', 1;

    INSERT INTO [AppDbo].[shipment_item] ([shipment_id], [order_item_id], [quantity], [JsonData])
    VALUES (@shipment_id, @order_item_id, @quantity, @JsonData);

    COMMIT TRANSACTION;

    EXEC [App].[shipment-get] @shipment_id = @shipment_id;
END;
