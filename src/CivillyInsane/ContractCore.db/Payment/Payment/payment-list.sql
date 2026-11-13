CREATE PROCEDURE [App].[payment-list]
    @order_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT  *
    FROM    [AppDbo].[payment-read]
    WHERE   [order_id] = @order_id
    ORDER BY [created_utc], [payment_id];
END;
