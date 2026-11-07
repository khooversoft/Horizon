CREATE VIEW [App].[business-event-read]
AS
SELECT t.[BusinessEventId],
       t.[AggregateTypeCode],
       t.[AggregateId],
       t.[EventTypeCode],
       t.[ActorTypeCode],
       t.[ActorId],
       t.[ReasonCode],
       t.[CorrelationId],
       t.[ApplicationName],
       t.[RequestId],
       t.CreatedUtc
FROM [AppDbo].[BusinessEvent] t

WHERE [AppDbo].[is-admin](0, 0) = 1;
