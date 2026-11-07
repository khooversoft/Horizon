CREATE PROCEDURE [App].[inbox-message-get]
    @InboxMessageId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [InboxMessageId], [ProviderCode], [ExternalMessageId], [MessageTypeCode], [StatusCode], [AttemptCount], [ReceivedUtc], [ProcessedUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[inbox-message-read] WHERE [InboxMessageId] = @InboxMessageId;
END;
