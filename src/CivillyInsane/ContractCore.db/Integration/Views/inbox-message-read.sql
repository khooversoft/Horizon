CREATE VIEW [App].[inbox-message-read]
AS
SELECT t.[InboxMessageId],
       t.[ProviderCode],
       t.[ExternalMessageId],
       t.[MessageTypeCode],
       t.[StatusCode],
       t.[AttemptCount],
       t.[ReceivedUtc],
       t.[ProcessedUtc],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[InboxMessage] t

WHERE [AppDbo].[is-admin](0, 0) = 1;
