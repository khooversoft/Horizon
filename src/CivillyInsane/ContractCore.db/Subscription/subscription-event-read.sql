CREATE VIEW [App].[subscription-event-read]
AS
SELECT t.[SubscriptionEventId],
       r0.[SubscriptionId] AS [SubscriptionId],
       t.[EventTypeCode],
       t.[SalesOrderId],
       t.[Reason],
       t.[Actor],
       t.[CorrelationId],
       t.CreatedUtc
FROM [AppDbo].[SubscriptionEvent] t
LEFT JOIN [AppDbo].[Subscription] r0 ON r0.RowId = t.[SubscriptionRowId]
WHERE EXISTS (SELECT 1 FROM AppDbo.Subscription s WHERE s.RowId = t.SubscriptionRowId AND [AppDbo].[can-read-customer](s.CustomerRowId) = 1);
