CREATE VIEW [AppDbo].[shipment-read]
AS
SELECT  t.[shipment_id]
        ,t.[order_id]
        ,t.[carrier]
        ,t.[service_level]
        ,t.[tracking_number]
        ,t.[status]
        ,t.[shipped_utc]
        ,t.[delivered_utc]
        ,t.[created_utc]
        ,t.[modified_utc]
        ,t.[JsonData]
FROM    [AppDbo].[shipment] t;
