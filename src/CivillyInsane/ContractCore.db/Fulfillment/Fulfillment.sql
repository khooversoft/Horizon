CREATE TABLE [AppDbo].[Fulfillment]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [FulfillmentId] nvarchar(50) NOT NULL,
    [SalesOrderRowId] bigint NOT NULL,
    [ProviderCode] nvarchar(50) NOT NULL,
    [ProviderOrderId] nvarchar(200) NULL,
    [StatusCode] varchar(25) NOT NULL CONSTRAINT [DF_Fulfillment_StatusCode] DEFAULT ('submitted'),
    [TrackingNumber] nvarchar(200) NULL,
    [TrackingUri] nvarchar(2048) NULL,
    [CarrierCode] nvarchar(50) NULL,
    [SubmittedUtc] datetime2(3) NULL,
    [ShippedUtc] datetime2(3) NULL,
    [DeliveredUtc] datetime2(3) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Fulfillment_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Fulfillment_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Fulfillment] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Fulfillment_FulfillmentId] UNIQUE NONCLUSTERED ([FulfillmentId]),
    CONSTRAINT [CK_Fulfillment_BusinessId] CHECK (LEN(TRIM([FulfillmentId])) > 0),
    CONSTRAINT [CK_Fulfillment_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_Fulfillment_SalesOrderRowId] FOREIGN KEY ([SalesOrderRowId]) REFERENCES [AppDbo].[SalesOrder] ([RowId]),
    CONSTRAINT [CK_Fulfillment_Rule1] CHECK (StatusCode IN ('submitted','accepted','processing','shipped','delivered','cancelled','failed'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Fulfillment_1] ON [AppDbo].[Fulfillment] (SalesOrderRowId,ProviderCode);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Fulfillment_2] ON [AppDbo].[Fulfillment] (ProviderCode,ProviderOrderId) WHERE ProviderOrderId IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Fulfillment_3] ON [AppDbo].[Fulfillment] (RowId,SalesOrderRowId);
GO
