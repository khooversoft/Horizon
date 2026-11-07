CREATE UNIQUE NONCLUSTERED INDEX [IX_Fulfillment_1] ON [AppDbo].[Fulfillment] (SalesOrderRowId,ProviderCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Fulfillment_2] ON [AppDbo].[Fulfillment] (ProviderCode,ProviderOrderId) WHERE ProviderOrderId IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Fulfillment_3] ON [AppDbo].[Fulfillment] (RowId,SalesOrderRowId);
GO
