CREATE PROCEDURE [App].[subscription-event-get]
    @SubscriptionEventId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SubscriptionEventId], [SubscriptionId], [EventTypeCode], [SalesOrderId], [Reason], [Actor], [CorrelationId], CreatedUtc FROM [App].[subscription-event-read] WHERE [SubscriptionEventId] = @SubscriptionEventId;
END;
