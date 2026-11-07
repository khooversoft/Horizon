CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomerIdentity_1] ON [AppDbo].[CustomerIdentity] (Issuer,Subject);
GO
CREATE NONCLUSTERED INDEX [IX_CustomerIdentity_2] ON [AppDbo].[CustomerIdentity] (CustomerRowId);
GO
