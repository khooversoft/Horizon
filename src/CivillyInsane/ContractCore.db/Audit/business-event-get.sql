CREATE PROCEDURE [App].[business-event-get]
    @BusinessEventId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [BusinessEventId], [AggregateTypeCode], [AggregateId], [EventTypeCode], [ActorTypeCode], [ActorId], [ReasonCode], [CorrelationId], [ApplicationName], [RequestId], CreatedUtc FROM [App].[business-event-read] WHERE [BusinessEventId] = @BusinessEventId;
END;
