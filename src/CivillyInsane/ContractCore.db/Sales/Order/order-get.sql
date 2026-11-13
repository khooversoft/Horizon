CREATE PROCEDURE [App].[order-get]
    @order_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT  *
    FROM    [AppDbo].[order-read]
    WHERE   [order_id] = @order_id;

    SELECT  *
    FROM    [AppDbo].[order-item-read]
    WHERE   [order_id] = @order_id
    ORDER BY [order_item_id];
END;
