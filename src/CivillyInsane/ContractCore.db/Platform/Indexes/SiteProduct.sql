CREATE UNIQUE NONCLUSTERED INDEX [IX_SiteProduct_1] ON [AppDbo].[SiteProduct] (SiteRowId,ProductRowId);
GO
CREATE NONCLUSTERED INDEX [IX_SiteProduct_2] ON [AppDbo].[SiteProduct] (SiteRowId,IsVisible,DisplaySequence);
GO
