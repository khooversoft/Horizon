CREATE TABLE [AppDbo].[FulfillmentItem]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [FulfillmentItemId] nvarchar(50) NOT NULL,
    [FulfillmentRowId] bigint NOT NULL,
    [SalesOrderRowId] bigint NOT NULL,
    [SalesOrderItemRowId] bigint NOT NULL,
    [Quantity] int NOT NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_FulfillmentItem_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_FulfillmentItem] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_FulfillmentItem_FulfillmentItemId] UNIQUE NONCLUSTERED ([FulfillmentItemId]),
    CONSTRAINT [CK_FulfillmentItem_BusinessId] CHECK (LEN(TRIM([FulfillmentItemId])) > 0),
    CONSTRAINT [CK_FulfillmentItem_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_FulfillmentItem_FulfillmentRowId] FOREIGN KEY ([FulfillmentRowId]) REFERENCES [AppDbo].[Fulfillment] ([RowId]),
    CONSTRAINT [FK_FulfillmentItem_SalesOrderRowId] FOREIGN KEY ([SalesOrderRowId]) REFERENCES [AppDbo].[SalesOrder] ([RowId]),
    CONSTRAINT [FK_FulfillmentItem_SalesOrderItemRowId] FOREIGN KEY ([SalesOrderItemRowId]) REFERENCES [AppDbo].[SalesOrderItem] ([RowId]),
    CONSTRAINT [CK_FulfillmentItem_Rule1] CHECK (Quantity > 0),
    CONSTRAINT [FK_FulfillmentItem_Fulfillment_Ownership] FOREIGN KEY (FulfillmentRowId,SalesOrderRowId) REFERENCES [AppDbo].[Fulfillment] (RowId,SalesOrderRowId),
    CONSTRAINT [FK_FulfillmentItem_SalesOrderItem_Ownership] FOREIGN KEY (SalesOrderItemRowId,SalesOrderRowId) REFERENCES [AppDbo].[SalesOrderItem] (RowId,SalesOrderRowId)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_FulfillmentItem_1] ON [AppDbo].[FulfillmentItem] (FulfillmentRowId,SalesOrderItemRowId);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_FulfillmentItem_2] ON [AppDbo].[FulfillmentItem] (SalesOrderItemRowId);
GO
