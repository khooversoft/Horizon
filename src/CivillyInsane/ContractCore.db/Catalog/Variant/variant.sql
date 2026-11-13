CREATE TABLE [AppDbo].[variant]
(
    [variant_id] bigint IDENTITY(1,1) NOT NULL,
    [product_id] bigint NOT NULL,
    [sku] varchar(64) NOT NULL,
    [size] varchar(20) NOT NULL,
    [color] varchar(40) NOT NULL,
    [price_cents] int NULL,
    [stock_qty] int NOT NULL CONSTRAINT [DF_variant_stock_qty] DEFAULT (0),
    [status] varchar(20) NOT NULL CONSTRAINT [DF_variant_status] DEFAULT ('active'),
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_variant_created_utc] DEFAULT (SYSUTCDATETIME()),
    [modified_utc] datetime2(3) NOT NULL CONSTRAINT [DF_variant_modified_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_variant] PRIMARY KEY CLUSTERED ([variant_id]),
    CONSTRAINT [UQ_variant_sku] UNIQUE NONCLUSTERED ([sku]),
    CONSTRAINT [UQ_variant_product_size_color] UNIQUE NONCLUSTERED ([product_id], [size], [color]),
    CONSTRAINT [FK_variant_product] FOREIGN KEY ([product_id]) REFERENCES [AppDbo].[product] ([product_id]),
    CONSTRAINT [CK_variant_sku] CHECK (LEN(TRIM([sku])) > 0),
    CONSTRAINT [CK_variant_price_cents] CHECK ([price_cents] IS NULL OR [price_cents] >= 0),
    CONSTRAINT [CK_variant_stock_qty] CHECK ([stock_qty] >= 0),
    CONSTRAINT [CK_variant_status] CHECK ([status] IN ('active','inactive')),
    CONSTRAINT [CK_variant_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO
