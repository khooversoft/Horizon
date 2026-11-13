CREATE TABLE [AppDbo].[payment]
(
    [payment_id] bigint IDENTITY(1,1) NOT NULL,
    [order_id] bigint NOT NULL,
    [provider] varchar(30) NOT NULL,
    [provider_ref] varchar(100) NULL,
    [status] varchar(20) NOT NULL CONSTRAINT [DF_payment_status] DEFAULT ('pending'),
    [amount_cents] int NOT NULL,
    [currency_code] char(3) NOT NULL CONSTRAINT [DF_payment_currency_code] DEFAULT ('USD'),
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_payment_created_utc] DEFAULT (SYSUTCDATETIME()),
    [modified_utc] datetime2(3) NOT NULL CONSTRAINT [DF_payment_modified_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_payment] PRIMARY KEY CLUSTERED ([payment_id]),
    CONSTRAINT [FK_payment_order] FOREIGN KEY ([order_id]) REFERENCES [AppDbo].[order] ([order_id]),
    CONSTRAINT [CK_payment_status] CHECK ([status] IN ('pending','authorized','captured','failed','refunded')),
    CONSTRAINT [CK_payment_amount_cents] CHECK ([amount_cents] >= 0),
    CONSTRAINT [CK_payment_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_payment_order_id]
    ON [AppDbo].[payment] ([order_id]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_payment_provider_ref]
    ON [AppDbo].[payment] ([provider], [provider_ref])
    WHERE [provider_ref] IS NOT NULL;
GO
