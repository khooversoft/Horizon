CREATE TABLE [AppDbo].[ProductCategory]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [ProductCategoryId] nvarchar(50) NOT NULL,
    [ProductRowId] bigint NOT NULL,
    [CategoryRowId] bigint NOT NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductCategory_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductCategory_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_ProductCategory] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_ProductCategory_ProductCategoryId] UNIQUE NONCLUSTERED ([ProductCategoryId]),
    CONSTRAINT [CK_ProductCategory_BusinessId] CHECK (LEN(TRIM([ProductCategoryId])) > 0),
    CONSTRAINT [CK_ProductCategory_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_ProductCategory_ProductRowId] FOREIGN KEY ([ProductRowId]) REFERENCES [AppDbo].[Product] ([RowId]),
    CONSTRAINT [FK_ProductCategory_CategoryRowId] FOREIGN KEY ([CategoryRowId]) REFERENCES [AppDbo].[Category] ([RowId])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductCategory_1] ON [AppDbo].[ProductCategory] (ProductRowId,CategoryRowId);
GO
