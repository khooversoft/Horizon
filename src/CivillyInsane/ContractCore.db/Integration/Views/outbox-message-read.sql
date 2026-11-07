CREATE VIEW [App].[outbox-message-read]
AS
SELECT t.[OutboxMessageId],
       t.[MessageTypeCode],
       t.[AggregateTypeCode],
       t.[AggregateId],
       t.[StatusCode],
       t.[AttemptCount],
       t.[PublishedUtc],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[OutboxMessage] t

WHERE [AppDbo].[is-admin](0, 0) = 1;
