CREATE VIEW [AppDbo].[variant-read]
AS
SELECT  t.[variant_id]
        ,t.[product_id]
        ,p.[name] AS [product_name]
        ,t.[sku]
        ,t.[size]
        ,t.[color]
        ,t.[price_cents]
        ,COALESCE(t.[price_cents], p.[base_price_cents]) AS [effective_price_cents]
        ,p.[currency_code]
        ,t.[stock_qty]
        ,t.[status]
        ,t.[created_utc]
        ,t.[modified_utc]
        ,t.[JsonData]
FROM    [AppDbo].[variant] t
JOIN    [AppDbo].[product] p ON p.[product_id] = t.[product_id];
