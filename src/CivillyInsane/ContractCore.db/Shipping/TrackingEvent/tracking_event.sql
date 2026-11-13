CREATE TABLE [AppDbo].[tracking_event]
(
    [tracking_event_id] bigint IDENTITY(1,1) NOT NULL,
    [shipment_id] bigint NOT NULL,
    [status_code] varchar(40) NOT NULL,
    [description] nvarchar(400) NULL,
    [location] nvarchar(200) NULL,
    [event_utc] datetime2(3) NOT NULL,
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_tracking_event_created_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_tracking_event] PRIMARY KEY CLUSTERED ([tracking_event_id]),
    CONSTRAINT [FK_tracking_event_shipment] FOREIGN KEY ([shipment_id]) REFERENCES [AppDbo].[shipment] ([shipment_id]),
    CONSTRAINT [CK_tracking_event_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO

CREATE NONCLUSTERED INDEX [IX_tracking_event_shipment_id]
    ON [AppDbo].[tracking_event] ([shipment_id], [event_utc]);
GO
