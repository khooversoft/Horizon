CREATE TABLE [AppDbo].[Customer]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [CustomerId] nvarchar(320) NOT NULL,
    [EmailDisplay] nvarchar(320) NOT NULL,
    [FirstName] nvarchar(100) NULL,
    [LastName] nvarchar(100) NULL,
    [Phone] nvarchar(50) NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_Customer_StatusCode] DEFAULT ('active'),
    [MarketingOptIn] bit NOT NULL CONSTRAINT [DF_Customer_MarketingOptIn] DEFAULT (0),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Customer_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Customer_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Customer_CustomerId] UNIQUE NONCLUSTERED ([CustomerId]),
    CONSTRAINT [CK_Customer_BusinessId] CHECK (LEN(TRIM([CustomerId])) > 0),
    CONSTRAINT [CK_Customer_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_Customer_Rule1] CHECK (CustomerId COLLATE Latin1_General_100_BIN2 = LOWER(TRIM(CustomerId)) COLLATE Latin1_General_100_BIN2 AND DATALENGTH(CustomerId) = DATALENGTH(TRIM(CustomerId)) AND CustomerId LIKE '%_@_%._%'),
    CONSTRAINT [CK_Customer_Rule2] CHECK (StatusCode IN ('active','blocked','closed'))
);
