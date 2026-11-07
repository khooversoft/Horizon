CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductVariant_1] ON [AppDbo].[ProductVariant] (Sku);
GO
CREATE NONCLUSTERED INDEX [IX_ProductVariant_2] ON [AppDbo].[ProductVariant] (ProductRowId,StatusCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductVariant_3] ON [AppDbo].[ProductVariant] (RowId,ProductRowId);
GO
