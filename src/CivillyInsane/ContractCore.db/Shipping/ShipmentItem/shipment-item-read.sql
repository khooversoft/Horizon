CREATE VIEW [AppDbo].[shipment-item-read]
AS
SELECT  t.[shipment_item_id]
        ,t.[shipment_id]
        ,t.[order_item_id]
        ,oi.[sku]
        ,oi.[product_name]
        ,oi.[size]
        ,oi.[color]
        ,t.[quantity]
        ,t.[created_utc]
        ,t.[JsonData]
FROM    [AppDbo].[shipment_item] t
JOIN    [AppDbo].[order_item] oi ON oi.[order_item_id] = t.[order_item_id];
