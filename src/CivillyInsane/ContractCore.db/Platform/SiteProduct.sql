CREATE TABLE [AppDbo].[SiteProduct]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SiteProductId] nvarchar(50) NOT NULL,
    [SiteRowId] bigint NOT NULL,
    [ProductRowId] bigint NOT NULL,
    [IsVisible] bit NOT NULL CONSTRAINT [DF_SiteProduct_IsVisible] DEFAULT (0),
    [DisplayNameOverride] nvarchar(250) NULL,
    [DisplaySequence] int NOT NULL CONSTRAINT [DF_SiteProduct_DisplaySequence] DEFAULT (0),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SiteProduct_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SiteProduct_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SiteProduct] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SiteProduct_SiteProductId] UNIQUE NONCLUSTERED ([SiteProductId]),
    CONSTRAINT [CK_SiteProduct_BusinessId] CHECK (LEN(TRIM([SiteProductId])) > 0),
    CONSTRAINT [CK_SiteProduct_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SiteProduct_SiteRowId] FOREIGN KEY ([SiteRowId]) REFERENCES [AppDbo].[Site] ([RowId]),
    CONSTRAINT [FK_SiteProduct_ProductRowId] FOREIGN KEY ([ProductRowId]) REFERENCES [AppDbo].[Product] ([RowId])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SiteProduct_1] ON [AppDbo].[SiteProduct] (SiteRowId,ProductRowId);
GO
CREATE NONCLUSTERED INDEX [IX_SiteProduct_2] ON [AppDbo].[SiteProduct] (SiteRowId,IsVisible,DisplaySequence);
GO
