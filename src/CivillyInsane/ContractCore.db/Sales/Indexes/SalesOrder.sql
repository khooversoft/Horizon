CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrder_1] ON [AppDbo].[SalesOrder] (OrderNumber);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrder_2] ON [AppDbo].[SalesOrder] (IdempotencyKey);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrder_3] ON [AppDbo].[SalesOrder] (SourceCartRowId) WHERE SourceCartRowId IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrder_4] ON [AppDbo].[SalesOrder] (SourceSubscriptionRowId,ScheduledProcessUtc) WHERE SourceSubscriptionRowId IS NOT NULL;
GO
CREATE NONCLUSTERED INDEX [IX_SalesOrder_5] ON [AppDbo].[SalesOrder] (CustomerRowId,PlacedUtc DESC);
GO
CREATE NONCLUSTERED INDEX [IX_SalesOrder_6] ON [AppDbo].[SalesOrder] (SiteRowId,OrderStatusCode,PlacedUtc);
GO
