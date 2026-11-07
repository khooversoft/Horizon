CREATE TABLE [AppDbo].[CustomerIdentity]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [CustomerIdentityId] nvarchar(50) NOT NULL,
    [CustomerRowId] bigint NOT NULL,
    [Issuer] nvarchar(500) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [Subject] nvarchar(200) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [IdentityProviderCode] nvarchar(50) NULL,
    [LastSeenUtc] datetime2(3) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CustomerIdentity_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_CustomerIdentity_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_CustomerIdentity] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_CustomerIdentity_CustomerIdentityId] UNIQUE NONCLUSTERED ([CustomerIdentityId]),
    CONSTRAINT [CK_CustomerIdentity_BusinessId] CHECK (LEN(TRIM([CustomerIdentityId])) > 0),
    CONSTRAINT [CK_CustomerIdentity_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_CustomerIdentity_CustomerRowId] FOREIGN KEY ([CustomerRowId]) REFERENCES [AppDbo].[Customer] ([RowId])
);
