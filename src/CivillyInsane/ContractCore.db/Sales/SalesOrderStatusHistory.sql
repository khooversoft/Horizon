CREATE TABLE [AppDbo].[SalesOrderStatusHistory]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SalesOrderStatusHistoryId] nvarchar(50) NOT NULL,
    [SalesOrderRowId] bigint NOT NULL,
    [FromStatus] varchar(25) NULL,
    [ToStatus] varchar(25) NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Actor] nvarchar(320) NOT NULL,
    [CorrelationId] uniqueidentifier NOT NULL,
    [ChangedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SalesOrderStatusHistory_ChangedUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SalesOrderStatusHistory_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SalesOrderStatusHistory] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SalesOrderStatusHistory_SalesOrderStatusHistoryId] UNIQUE NONCLUSTERED ([SalesOrderStatusHistoryId]),
    CONSTRAINT [CK_SalesOrderStatusHistory_BusinessId] CHECK (LEN(TRIM([SalesOrderStatusHistoryId])) > 0),
    CONSTRAINT [CK_SalesOrderStatusHistory_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SalesOrderStatusHistory_SalesOrderRowId] FOREIGN KEY ([SalesOrderRowId]) REFERENCES [AppDbo].[SalesOrder] ([RowId])
);
GO

CREATE NONCLUSTERED INDEX [IX_SalesOrderStatusHistory_1] ON [AppDbo].[SalesOrderStatusHistory] (SalesOrderRowId,ChangedUtc);
GO
