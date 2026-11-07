CREATE TABLE [AppDbo].[Country]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [CountryId] nvarchar(50) NOT NULL,
    [CountryCode] char(2) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [IsSupported] bit NOT NULL CONSTRAINT [DF_Country_IsSupported] DEFAULT (1),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Country_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Country_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Country_CountryId] UNIQUE NONCLUSTERED ([CountryId]),
    CONSTRAINT [CK_Country_BusinessId] CHECK (LEN(TRIM([CountryId])) > 0),
    CONSTRAINT [CK_Country_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_Country_Rule1] CHECK (CountryCode COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z]%')
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Country_1] ON [AppDbo].[Country] (CountryCode);
GO
