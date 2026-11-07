CREATE NONCLUSTERED INDEX [IX_BusinessEvent_1] ON [AppDbo].[BusinessEvent] (AggregateTypeCode,AggregateId,CreatedUtc);
GO
CREATE NONCLUSTERED INDEX [IX_BusinessEvent_2] ON [AppDbo].[BusinessEvent] (CorrelationId);
GO
