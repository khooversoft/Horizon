CREATE NONCLUSTERED INDEX [IX_CustomerAddress_1] ON [AppDbo].[CustomerAddress] (CustomerRowId,StatusCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomerAddress_2] ON [AppDbo].[CustomerAddress] (CustomerRowId) WHERE IsDefaultShipping = 1 AND StatusCode = 'active';
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomerAddress_3] ON [AppDbo].[CustomerAddress] (CustomerRowId) WHERE IsDefaultBilling = 1 AND StatusCode = 'active';
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomerAddress_4] ON [AppDbo].[CustomerAddress] (RowId,CustomerRowId);
GO
