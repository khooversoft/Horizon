CREATE VIEW [AppDbo].[customer-read]
AS
SELECT  t.[customer_id]
        ,t.[email]
        ,t.[first_name]
        ,t.[last_name]
        ,t.[phone]
        ,t.[created_utc]
        ,t.[modified_utc]
        ,t.[JsonData]
FROM    [AppDbo].[customer] t;
