CREATE TABLE [AppDbo].[ProductVariantOptionValue]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [ProductVariantOptionValueId] nvarchar(50) NOT NULL,
    [ProductVariantRowId] bigint NOT NULL,
    [ProductOptionRowId] bigint NOT NULL,
    [ProductOptionValueRowId] bigint NOT NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductVariantOptionValue_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductVariantOptionValue_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_ProductVariantOptionValue] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_ProductVariantOptionValue_ProductVariantOptionValueId] UNIQUE NONCLUSTERED ([ProductVariantOptionValueId]),
    CONSTRAINT [CK_ProductVariantOptionValue_BusinessId] CHECK (LEN(TRIM([ProductVariantOptionValueId])) > 0),
    CONSTRAINT [CK_ProductVariantOptionValue_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_ProductVariantOptionValue_ProductVariantRowId] FOREIGN KEY ([ProductVariantRowId]) REFERENCES [AppDbo].[ProductVariant] ([RowId]),
    CONSTRAINT [FK_ProductVariantOptionValue_ProductOptionRowId] FOREIGN KEY ([ProductOptionRowId]) REFERENCES [AppDbo].[ProductOption] ([RowId]),
    CONSTRAINT [FK_ProductVariantOptionValue_ProductOptionValueRowId] FOREIGN KEY ([ProductOptionValueRowId]) REFERENCES [AppDbo].[ProductOptionValue] ([RowId]),
    CONSTRAINT [FK_ProductVariantOptionValue_ProductOptionValue_Ownership] FOREIGN KEY (ProductOptionValueRowId,ProductOptionRowId) REFERENCES [AppDbo].[ProductOptionValue] (RowId,ProductOptionRowId)
);
