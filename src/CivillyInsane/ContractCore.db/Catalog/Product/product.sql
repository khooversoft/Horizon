CREATE TABLE [AppDbo].[product]
(
    [product_id] bigint IDENTITY(1,1) NOT NULL,
    [slug] varchar(100) NOT NULL,
    [name] nvarchar(200) NOT NULL,
    [description] nvarchar(max) NULL,
    [base_price_cents] int NOT NULL,
    [currency_code] char(3) NOT NULL CONSTRAINT [DF_product_currency_code] DEFAULT ('USD'),
    [status] varchar(20) NOT NULL CONSTRAINT [DF_product_status] DEFAULT ('draft'),
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_product_created_utc] DEFAULT (SYSUTCDATETIME()),
    [modified_utc] datetime2(3) NOT NULL CONSTRAINT [DF_product_modified_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_product] PRIMARY KEY CLUSTERED ([product_id]),
    CONSTRAINT [UQ_product_slug] UNIQUE NONCLUSTERED ([slug]),
    CONSTRAINT [CK_product_slug] CHECK (LEN(TRIM([slug])) > 0),
    CONSTRAINT [CK_product_base_price_cents] CHECK ([base_price_cents] >= 0),
    CONSTRAINT [CK_product_status] CHECK ([status] IN ('draft','active','archived')),
    CONSTRAINT [CK_product_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_product_status]
    ON [AppDbo].[product] ([status]);
GO
