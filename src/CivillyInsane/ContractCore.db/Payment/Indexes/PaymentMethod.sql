CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentMethod_1] ON [AppDbo].[PaymentMethod] (ProviderCode,ProviderPaymentMethodId);
GO
CREATE NONCLUSTERED INDEX [IX_PaymentMethod_2] ON [AppDbo].[PaymentMethod] (CustomerRowId,StatusCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentMethod_3] ON [AppDbo].[PaymentMethod] (CustomerRowId) WHERE IsDefault = 1 AND StatusCode = 'active';
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentMethod_4] ON [AppDbo].[PaymentMethod] (RowId,CustomerRowId);
GO
