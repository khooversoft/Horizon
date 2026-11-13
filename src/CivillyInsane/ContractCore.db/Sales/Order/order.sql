CREATE TABLE [AppDbo].[order]
(
    [order_id] bigint IDENTITY(1,1) NOT NULL,
    [customer_id] bigint NULL,
    [email] nvarchar(320) NOT NULL,
    [status] varchar(20) NOT NULL CONSTRAINT [DF_order_status] DEFAULT ('pending'),
    [currency_code] char(3) NOT NULL CONSTRAINT [DF_order_currency_code] DEFAULT ('USD'),
    [subtotal_cents] int NOT NULL CONSTRAINT [DF_order_subtotal_cents] DEFAULT (0),
    [shipping_cents] int NOT NULL CONSTRAINT [DF_order_shipping_cents] DEFAULT (0),
    [tax_cents] int NOT NULL CONSTRAINT [DF_order_tax_cents] DEFAULT (0),
    [discount_cents] int NOT NULL CONSTRAINT [DF_order_discount_cents] DEFAULT (0),
    [total_cents] AS ([subtotal_cents] + [shipping_cents] + [tax_cents] - [discount_cents]) PERSISTED,
    [ship_name] nvarchar(200) NULL,
    [ship_line1] nvarchar(200) NULL,
    [ship_line2] nvarchar(200) NULL,
    [ship_city] nvarchar(100) NULL,
    [ship_region] nvarchar(100) NULL,
    [ship_postal_code] varchar(20) NULL,
    [ship_country_code] char(2) NULL,
    [placed_utc] datetime2(3) NULL,
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_order_created_utc] DEFAULT (SYSUTCDATETIME()),
    [modified_utc] datetime2(3) NOT NULL CONSTRAINT [DF_order_modified_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_order] PRIMARY KEY CLUSTERED ([order_id]),
    CONSTRAINT [FK_order_customer] FOREIGN KEY ([customer_id]) REFERENCES [AppDbo].[customer] ([customer_id]),
    CONSTRAINT [CK_order_email] CHECK ([email] LIKE '%_@_%'),
    CONSTRAINT [CK_order_status] CHECK ([status] IN ('pending','paid','partially_shipped','shipped','delivered','cancelled','refunded')),
    CONSTRAINT [CK_order_amounts] CHECK ([subtotal_cents] >= 0 AND [shipping_cents] >= 0 AND [tax_cents] >= 0 AND [discount_cents] >= 0),
    CONSTRAINT [CK_order_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_order_customer_id]
    ON [AppDbo].[order] ([customer_id]);
GO
CREATE NONCLUSTERED INDEX [IX_order_email]
    ON [AppDbo].[order] ([email]);
GO
CREATE NONCLUSTERED INDEX [IX_order_status]
    ON [AppDbo].[order] ([status], [created_utc]);
GO
