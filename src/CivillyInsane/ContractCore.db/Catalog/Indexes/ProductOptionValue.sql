CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductOptionValue_1] ON [AppDbo].[ProductOptionValue] (ProductOptionRowId,ValueCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductOptionValue_2] ON [AppDbo].[ProductOptionValue] (RowId,ProductOptionRowId);
GO
