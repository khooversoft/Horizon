CREATE UNIQUE NONCLUSTERED INDEX [IX_Payment_1] ON [AppDbo].[Payment] (SalesOrderRowId);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Payment_2] ON [AppDbo].[Payment] (ProviderCode,ProviderPaymentId) WHERE ProviderPaymentId IS NOT NULL;
GO
