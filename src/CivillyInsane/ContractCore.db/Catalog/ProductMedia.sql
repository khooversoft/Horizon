CREATE TABLE [AppDbo].[ProductMedia]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [ProductMediaId] nvarchar(50) NOT NULL,
    [ProductRowId] bigint NOT NULL,
    [ProductVariantRowId] bigint NULL,
    [MediaTypeCode] varchar(20) NOT NULL,
    [StorageUri] nvarchar(2048) NOT NULL,
    [ThumbnailUri] nvarchar(2048) NULL,
    [AltText] nvarchar(500) NULL,
    [DisplaySequence] int NOT NULL CONSTRAINT [DF_ProductMedia_DisplaySequence] DEFAULT (0),
    [IsPrimary] bit NOT NULL CONSTRAINT [DF_ProductMedia_IsPrimary] DEFAULT (0),
    [ContentType] nvarchar(100) NOT NULL,
    [Width] int NULL,
    [Height] int NULL,
    [DurationSeconds] decimal(12,3) NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_ProductMedia_StatusCode] DEFAULT ('active'),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductMedia_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductMedia_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_ProductMedia] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_ProductMedia_ProductMediaId] UNIQUE NONCLUSTERED ([ProductMediaId]),
    CONSTRAINT [CK_ProductMedia_BusinessId] CHECK (LEN(TRIM([ProductMediaId])) > 0),
    CONSTRAINT [CK_ProductMedia_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_ProductMedia_ProductRowId] FOREIGN KEY ([ProductRowId]) REFERENCES [AppDbo].[Product] ([RowId]),
    CONSTRAINT [FK_ProductMedia_ProductVariantRowId] FOREIGN KEY ([ProductVariantRowId]) REFERENCES [AppDbo].[ProductVariant] ([RowId]),
    CONSTRAINT [CK_ProductMedia_Rule1] CHECK (MediaTypeCode IN ('image','video','thumbnail','size-chart','document')),
    CONSTRAINT [CK_ProductMedia_Rule2] CHECK (MediaTypeCode <> 'image' OR LEN(TRIM(AltText)) > 0 AND AltText IS NOT NULL),
    CONSTRAINT [CK_ProductMedia_Rule3] CHECK (StatusCode IN ('active','archived')),
    CONSTRAINT [CK_ProductMedia_Rule4] CHECK (Width IS NULL OR Width > 0),
    CONSTRAINT [CK_ProductMedia_Rule5] CHECK (Height IS NULL OR Height > 0),
    CONSTRAINT [CK_ProductMedia_Rule6] CHECK (DurationSeconds IS NULL OR DurationSeconds >= 0),
    CONSTRAINT [FK_ProductMedia_ProductVariant_Ownership] FOREIGN KEY (ProductVariantRowId,ProductRowId) REFERENCES [AppDbo].[ProductVariant] (RowId,ProductRowId)
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductMedia_1] ON [AppDbo].[ProductMedia] (ProductRowId,ProductVariantRowId,DisplaySequence);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ProductMedia_2] ON [AppDbo].[ProductMedia] (ProductRowId,ProductVariantRowId,MediaTypeCode) WHERE IsPrimary = 1 AND StatusCode = 'active';
GO
