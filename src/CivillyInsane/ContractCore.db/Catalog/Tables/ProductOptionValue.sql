CREATE TABLE [AppDbo].[ProductOptionValue]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [ProductOptionValueId] nvarchar(50) NOT NULL,
    [ProductOptionRowId] bigint NOT NULL,
    [ValueCode] nvarchar(50) NOT NULL,
    [DisplayValue] nvarchar(150) NOT NULL,
    [DisplaySequence] int NOT NULL CONSTRAINT [DF_ProductOptionValue_DisplaySequence] DEFAULT (0),
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_ProductOptionValue_StatusCode] DEFAULT ('active'),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductOptionValue_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_ProductOptionValue_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_ProductOptionValue] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_ProductOptionValue_ProductOptionValueId] UNIQUE NONCLUSTERED ([ProductOptionValueId]),
    CONSTRAINT [CK_ProductOptionValue_BusinessId] CHECK (LEN(TRIM([ProductOptionValueId])) > 0),
    CONSTRAINT [CK_ProductOptionValue_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_ProductOptionValue_ProductOptionRowId] FOREIGN KEY ([ProductOptionRowId]) REFERENCES [AppDbo].[ProductOption] ([RowId]),
    CONSTRAINT [CK_ProductOptionValue_Rule1] CHECK (StatusCode IN ('active','inactive'))
);
