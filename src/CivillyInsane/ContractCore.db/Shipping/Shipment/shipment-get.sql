CREATE PROCEDURE [App].[shipment-get]
    @shipment_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT  *
    FROM    [AppDbo].[shipment-read]
    WHERE   [shipment_id] = @shipment_id;

    SELECT  *
    FROM    [AppDbo].[shipment-item-read]
    WHERE   [shipment_id] = @shipment_id
    ORDER BY [shipment_item_id];

    SELECT  *
    FROM    [AppDbo].[tracking-event-read]
    WHERE   [shipment_id] = @shipment_id
    ORDER BY [event_utc], [tracking_event_id];
END;
