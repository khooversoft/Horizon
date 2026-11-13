CREATE VIEW [AppDbo].[payment-read]
AS
SELECT  t.[payment_id]
        ,t.[order_id]
        ,t.[provider]
        ,t.[provider_ref]
        ,t.[status]
        ,t.[amount_cents]
        ,t.[currency_code]
        ,t.[created_utc]
        ,t.[modified_utc]
        ,t.[JsonData]
FROM    [AppDbo].[payment] t;
