CREATE TABLE [AppDbo].[SubscriptionEvent]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SubscriptionEventId] nvarchar(50) NOT NULL,
    [SubscriptionRowId] bigint NOT NULL,
    [EventTypeCode] varchar(50) NOT NULL,
    [SalesOrderId] nvarchar(50) NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Actor] nvarchar(320) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [EventDataJson] varchar(max) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SubscriptionEvent_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SubscriptionEvent] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SubscriptionEvent_SubscriptionEventId] UNIQUE NONCLUSTERED ([SubscriptionEventId]),
    CONSTRAINT [CK_SubscriptionEvent_BusinessId] CHECK (LEN(TRIM([SubscriptionEventId])) > 0),
    CONSTRAINT [CK_SubscriptionEvent_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SubscriptionEvent_SubscriptionRowId] FOREIGN KEY ([SubscriptionRowId]) REFERENCES [AppDbo].[Subscription] ([RowId]),
    CONSTRAINT [FK_SubscriptionEvent_SalesOrderId] FOREIGN KEY ([SalesOrderId]) REFERENCES [AppDbo].[SalesOrder] ([SalesOrderId]),
    CONSTRAINT [CK_SubscriptionEvent_EventDataJson] CHECK ([EventDataJson] IS NULL OR ISJSON([EventDataJson]) = 1)
);
