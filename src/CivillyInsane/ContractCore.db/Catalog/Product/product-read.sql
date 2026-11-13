CREATE VIEW [AppDbo].[product-read]
AS
SELECT  t.[product_id]
        ,t.[slug]
        ,t.[name]
        ,t.[description]
        ,t.[base_price_cents]
        ,t.[currency_code]
        ,t.[status]
        ,t.[created_utc]
        ,t.[modified_utc]
        ,t.[JsonData]
FROM    [AppDbo].[product] t;
