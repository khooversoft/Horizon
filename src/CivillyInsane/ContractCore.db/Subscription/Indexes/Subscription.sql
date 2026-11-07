CREATE NONCLUSTERED INDEX [IX_Subscription_1] ON [AppDbo].[Subscription] (StatusCode,NextProcessUtc);
GO
CREATE NONCLUSTERED INDEX [IX_Subscription_2] ON [AppDbo].[Subscription] (CustomerRowId,StatusCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subscription_3] ON [AppDbo].[Subscription] (RowId,CustomerRowId);
GO
