CREATE TABLE [AppDbo].[Site]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SiteId] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [PrimaryHost] nvarchar(255) NOT NULL,
    [DefaultCurrencyCode] char(3) NOT NULL,
    [DefaultFulfillmentProviderCode] nvarchar(50) NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_Site_StatusCode] DEFAULT ('draft'),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Site_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Site_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Site] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Site_SiteId] UNIQUE NONCLUSTERED ([SiteId]),
    CONSTRAINT [CK_Site_BusinessId] CHECK (LEN(TRIM([SiteId])) > 0),
    CONSTRAINT [CK_Site_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_Site_DefaultCurrencyCode] FOREIGN KEY ([DefaultCurrencyCode]) REFERENCES [AppDbo].[Currency] ([CurrencyCode]),
    CONSTRAINT [CK_Site_Rule1] CHECK (StatusCode IN ('draft','active','suspended','closed'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Site_1] ON [AppDbo].[Site] (PrimaryHost);
GO
