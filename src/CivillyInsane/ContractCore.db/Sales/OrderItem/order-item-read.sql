CREATE VIEW [AppDbo].[order-item-read]
AS
SELECT  t.[order_item_id]
        ,t.[order_id]
        ,t.[variant_id]
        ,t.[sku]
        ,t.[product_name]
        ,t.[size]
        ,t.[color]
        ,t.[unit_price_cents]
        ,t.[quantity]
        ,t.[line_total_cents]
        ,COALESCE(s.[shipped_qty], 0) AS [shipped_qty]
        ,t.[created_utc]
        ,t.[JsonData]
FROM    [AppDbo].[order_item] t
LEFT JOIN
(
    SELECT  [order_item_id], SUM([quantity]) AS [shipped_qty]
    FROM    [AppDbo].[shipment_item]
    GROUP BY [order_item_id]
) s ON s.[order_item_id] = t.[order_item_id];
