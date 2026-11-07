CREATE NONCLUSTERED INDEX [IX_ProductMedia_1] ON [AppDbo].[ProductMedia] (ProductRowId,ProductVariantRowId,DisplaySequence);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductMedia_2] ON [AppDbo].[ProductMedia] (ProductRowId,ProductVariantRowId,MediaTypeCode) WHERE IsPrimary = 1 AND StatusCode = 'active';
GO
