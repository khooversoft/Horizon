CREATE PROCEDURE [App].[outbox-message-get]
    @OutboxMessageId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [OutboxMessageId], [MessageTypeCode], [AggregateTypeCode], [AggregateId], [StatusCode], [AttemptCount], [PublishedUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[outbox-message-read] WHERE [OutboxMessageId] = @OutboxMessageId;
END;
