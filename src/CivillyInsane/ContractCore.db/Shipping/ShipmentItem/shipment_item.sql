CREATE TABLE [AppDbo].[shipment_item]
(
    [shipment_item_id] bigint IDENTITY(1,1) NOT NULL,
    [shipment_id] bigint NOT NULL,
    [order_item_id] bigint NOT NULL,
    [quantity] int NOT NULL,
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_shipment_item_created_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_shipment_item] PRIMARY KEY CLUSTERED ([shipment_item_id]),
    CONSTRAINT [UQ_shipment_item_shipment_order_item] UNIQUE NONCLUSTERED ([shipment_id], [order_item_id]),
    CONSTRAINT [FK_shipment_item_shipment] FOREIGN KEY ([shipment_id]) REFERENCES [AppDbo].[shipment] ([shipment_id]),
    CONSTRAINT [FK_shipment_item_order_item] FOREIGN KEY ([order_item_id]) REFERENCES [AppDbo].[order_item] ([order_item_id]),
    CONSTRAINT [CK_shipment_item_quantity] CHECK ([quantity] > 0),
    CONSTRAINT [CK_shipment_item_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_shipment_item_order_item_id]
    ON [AppDbo].[shipment_item] ([order_item_id]) INCLUDE ([quantity]);
GO
