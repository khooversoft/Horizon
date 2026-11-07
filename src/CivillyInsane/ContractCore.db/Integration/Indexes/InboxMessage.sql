CREATE UNIQUE NONCLUSTERED INDEX [IX_InboxMessage_1] ON [AppDbo].[InboxMessage] (ProviderCode,ExternalMessageId);
GO
CREATE NONCLUSTERED INDEX [IX_InboxMessage_2] ON [AppDbo].[InboxMessage] (StatusCode,ReceivedUtc);
GO
