CREATE UNIQUE NONCLUSTERED INDEX [IX_Cart_1] ON [AppDbo].[Cart] (SiteRowId,CustomerRowId) WHERE StatusCode = 'active' AND CustomerRowId IS NOT NULL;
GO
CREATE NONCLUSTERED INDEX [IX_Cart_2] ON [AppDbo].[Cart] (GuestTokenHash,StatusCode);
GO
CREATE NONCLUSTERED INDEX [IX_Cart_3] ON [AppDbo].[Cart] (StatusCode,ExpiresUtc);
GO
