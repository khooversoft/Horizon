CREATE TABLE [AppDbo].[SubscriptionItem]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SubscriptionItemId] nvarchar(50) NOT NULL,
    [SubscriptionRowId] bigint NOT NULL,
    [ProductVariantRowId] bigint NOT NULL,
    [Quantity] int NOT NULL,
    [PriceCeiling] decimal(19,4) NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_SubscriptionItem_StatusCode] DEFAULT ('active'),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SubscriptionItem_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SubscriptionItem_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SubscriptionItem] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SubscriptionItem_SubscriptionItemId] UNIQUE NONCLUSTERED ([SubscriptionItemId]),
    CONSTRAINT [CK_SubscriptionItem_BusinessId] CHECK (LEN(TRIM([SubscriptionItemId])) > 0),
    CONSTRAINT [CK_SubscriptionItem_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SubscriptionItem_SubscriptionRowId] FOREIGN KEY ([SubscriptionRowId]) REFERENCES [AppDbo].[Subscription] ([RowId]),
    CONSTRAINT [FK_SubscriptionItem_ProductVariantRowId] FOREIGN KEY ([ProductVariantRowId]) REFERENCES [AppDbo].[ProductVariant] ([RowId]),
    CONSTRAINT [CK_SubscriptionItem_Rule1] CHECK (Quantity > 0),
    CONSTRAINT [CK_SubscriptionItem_Rule2] CHECK (PriceCeiling IS NULL OR PriceCeiling >= 0),
    CONSTRAINT [CK_SubscriptionItem_Rule3] CHECK (StatusCode IN ('active','inactive'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SubscriptionItem_1] ON [AppDbo].[SubscriptionItem] (SubscriptionRowId,ProductVariantRowId);
GO
