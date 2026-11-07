CREATE NONCLUSTERED INDEX [IX_SalesOrderItem_1] ON [AppDbo].[SalesOrderItem] (SalesOrderRowId);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrderItem_2] ON [AppDbo].[SalesOrderItem] (RowId,SalesOrderRowId);
GO
