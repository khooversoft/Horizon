CREATE PROCEDURE [App].[shipment-list]
    @order_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT  *
    FROM    [AppDbo].[shipment-read]
    WHERE   [order_id] = @order_id
    ORDER BY [created_utc], [shipment_id];
END;
