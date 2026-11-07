CREATE NONCLUSTERED INDEX [IX_OutboxMessage_1] ON [AppDbo].[OutboxMessage] (StatusCode,CreatedUtc);
GO
