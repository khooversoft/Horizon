CREATE TABLE [AppDbo].[ProductOption]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [ProductOptionId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [DataTypeCode] varchar(20) NOT NULL CONSTRAINT [DF_ProductOption_DataTypeCode] DEFAULT ('text'),
    [DisplaySequence] int NOT NULL CONSTRAINT [DF_ProductOption_DisplaySequence] DEFAULT (0),
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_ProductOption_StatusCode] DEFAULT ('active'),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductOption_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductOption_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_ProductOption] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_ProductOption_ProductOptionId] UNIQUE NONCLUSTERED ([ProductOptionId]),
    CONSTRAINT [CK_ProductOption_BusinessId] CHECK (LEN(TRIM([ProductOptionId])) > 0),
    CONSTRAINT [CK_ProductOption_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_ProductOption_Rule1] CHECK (StatusCode IN ('active','inactive'))
);
