CREATE TABLE [AppDbo].[SalesOrderDiscount]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SalesOrderDiscountId] nvarchar(50) NOT NULL,
    [SalesOrderRowId] bigint NOT NULL,
    [Code] nvarchar(50) NULL,
    [Description] nvarchar(250) NOT NULL,
    [DiscountTypeCode] varchar(20) NOT NULL,
    [Amount] decimal(19,4) NOT NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SalesOrderDiscount_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SalesOrderDiscount] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SalesOrderDiscount_SalesOrderDiscountId] UNIQUE NONCLUSTERED ([SalesOrderDiscountId]),
    CONSTRAINT [CK_SalesOrderDiscount_BusinessId] CHECK (LEN(TRIM([SalesOrderDiscountId])) > 0),
    CONSTRAINT [CK_SalesOrderDiscount_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SalesOrderDiscount_SalesOrderRowId] FOREIGN KEY ([SalesOrderRowId]) REFERENCES [AppDbo].[SalesOrder] ([RowId]),
    CONSTRAINT [CK_SalesOrderDiscount_Rule1] CHECK (Amount >= 0)
);
