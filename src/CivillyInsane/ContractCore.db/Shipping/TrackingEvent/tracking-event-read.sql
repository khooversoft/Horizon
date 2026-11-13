CREATE VIEW [AppDbo].[tracking-event-read]
AS
SELECT  t.[tracking_event_id]
        ,t.[shipment_id]
        ,t.[status_code]
        ,t.[description]
        ,t.[location]
        ,t.[event_utc]
        ,t.[created_utc]
        ,t.[JsonData]
FROM    [AppDbo].[tracking_event] t;
