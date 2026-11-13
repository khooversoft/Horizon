CREATE TABLE [AppDbo].[order_item]
(
    [order_item_id] bigint IDENTITY(1,1) NOT NULL,
    [order_id] bigint NOT NULL,
    [variant_id] bigint NOT NULL,
    [sku] varchar(64) NOT NULL,
    [product_name] nvarchar(200) NOT NULL,
    [size] varchar(20) NOT NULL,
    [color] varchar(40) NOT NULL,
    [unit_price_cents] int NOT NULL,
    [quantity] int NOT NULL,
    [line_total_cents] AS ([unit_price_cents] * [quantity]) PERSISTED,
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_order_item_created_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_order_item] PRIMARY KEY CLUSTERED ([order_item_id]),
    CONSTRAINT [FK_order_item_order] FOREIGN KEY ([order_id]) REFERENCES [AppDbo].[order] ([order_id]),
    CONSTRAINT [FK_order_item_variant] FOREIGN KEY ([variant_id]) REFERENCES [AppDbo].[variant] ([variant_id]),
    CONSTRAINT [CK_order_item_unit_price_cents] CHECK ([unit_price_cents] >= 0),
    CONSTRAINT [CK_order_item_quantity] CHECK ([quantity] > 0),
    CONSTRAINT [CK_order_item_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_order_item_order_id]
    ON [AppDbo].[order_item] ([order_id]);
GO
