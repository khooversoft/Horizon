CREATE TABLE [AppDbo].[CartItem]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [CartItemId] nvarchar(50) NOT NULL,
    [CartRowId] bigint NOT NULL,
    [ProductVariantRowId] bigint NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPriceEstimate] decimal(19,4) NOT NULL,
    [AddedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CartItem_AddedUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CartItem_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CartItem_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_CartItem] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_CartItem_CartItemId] UNIQUE NONCLUSTERED ([CartItemId]),
    CONSTRAINT [CK_CartItem_BusinessId] CHECK (LEN(TRIM([CartItemId])) > 0),
    CONSTRAINT [CK_CartItem_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_CartItem_CartRowId] FOREIGN KEY ([CartRowId]) REFERENCES [AppDbo].[Cart] ([RowId]),
    CONSTRAINT [FK_CartItem_ProductVariantRowId] FOREIGN KEY ([ProductVariantRowId]) REFERENCES [AppDbo].[ProductVariant] ([RowId]),
    CONSTRAINT [CK_CartItem_Rule1] CHECK (Quantity > 0),
    CONSTRAINT [CK_CartItem_Rule2] CHECK (UnitPriceEstimate >= 0)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_CartItem_1] ON [AppDbo].[CartItem] (CartRowId,ProductVariantRowId);
GO
