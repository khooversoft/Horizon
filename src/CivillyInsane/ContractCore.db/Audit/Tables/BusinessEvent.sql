CREATE TABLE [AppDbo].[BusinessEvent]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [BusinessEventId] nvarchar(50) NOT NULL,
    [AggregateTypeCode] nvarchar(50) NOT NULL,
    [AggregateId] nvarchar(320) NOT NULL,
    [EventTypeCode] varchar(100) NOT NULL,
    [ActorTypeCode] varchar(20) NOT NULL,
    [ActorId] nvarchar(320) NOT NULL,
    [ReasonCode] nvarchar(500) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [ApplicationName] nvarchar(128) NULL,
    [RequestId] nvarchar(100) NULL,
    [EventDataJson] varchar(max) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_BusinessEvent_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_BusinessEvent] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_BusinessEvent_BusinessEventId] UNIQUE NONCLUSTERED ([BusinessEventId]),
    CONSTRAINT [CK_BusinessEvent_BusinessId] CHECK (LEN(TRIM([BusinessEventId])) > 0),
    CONSTRAINT [CK_BusinessEvent_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_BusinessEvent_EventDataJson] CHECK ([EventDataJson] IS NULL OR ISJSON([EventDataJson]) = 1)
);
