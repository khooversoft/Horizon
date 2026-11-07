CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentTransaction_1] ON [AppDbo].[PaymentTransaction] (ProviderCode,ProviderEventId) WHERE ProviderEventId IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentTransaction_2] ON [AppDbo].[PaymentTransaction] (ProviderCode,ProviderTransactionId,TransactionTypeCode,StatusCode) WHERE ProviderTransactionId IS NOT NULL;
GO
CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_3] ON [AppDbo].[PaymentTransaction] (PaymentRowId,OccurredUtc);
GO
