CREATE TABLE [AppDbo].[Currency]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [CurrencyId] nvarchar(50) NOT NULL,
    [CurrencyCode] char(3) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [DecimalPlaces] tinyint NOT NULL CONSTRAINT [DF_Currency_DecimalPlaces] DEFAULT (2),
    [IsSupported] bit NOT NULL CONSTRAINT [DF_Currency_IsSupported] DEFAULT (1),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Currency_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Currency_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Currency] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Currency_CurrencyId] UNIQUE NONCLUSTERED ([CurrencyId]),
    CONSTRAINT [CK_Currency_BusinessId] CHECK (LEN(TRIM([CurrencyId])) > 0),
    CONSTRAINT [CK_Currency_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_Currency_Rule1] CHECK (DecimalPlaces <= 4),
    CONSTRAINT [CK_Currency_Rule2] CHECK (CurrencyCode COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z]%')
);
