CREATE TABLE [AppDbo].[shipment]
(
    [shipment_id] bigint IDENTITY(1,1) NOT NULL,
    [order_id] bigint NOT NULL,
    [carrier] varchar(30) NULL,
    [service_level] varchar(50) NULL,
    [tracking_number] varchar(100) NULL,
    [status] varchar(20) NOT NULL CONSTRAINT [DF_shipment_status] DEFAULT ('pending'),
    [shipped_utc] datetime2(3) NULL,
    [delivered_utc] datetime2(3) NULL,
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_shipment_created_utc] DEFAULT (SYSUTCDATETIME()),
    [modified_utc] datetime2(3) NOT NULL CONSTRAINT [DF_shipment_modified_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_shipment] PRIMARY KEY CLUSTERED ([shipment_id]),
    CONSTRAINT [FK_shipment_order] FOREIGN KEY ([order_id]) REFERENCES [AppDbo].[order] ([order_id]),
    CONSTRAINT [CK_shipment_status] CHECK ([status] IN ('pending','shipped','in_transit','delivered','returned','exception')),
    CONSTRAINT [CK_shipment_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_shipment_order_id]
    ON [AppDbo].[shipment] ([order_id]);
GO
CREATE NONCLUSTERED INDEX [IX_shipment_tracking_number]
    ON [AppDbo].[shipment] ([tracking_number])
    WHERE [tracking_number] IS NOT NULL;
GO
