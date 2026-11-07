CREATE TABLE [AppDbo].[Category]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [CategoryId] nvarchar(50) NOT NULL,
    [ParentCategoryRowId] bigint NULL,
    [Name] nvarchar(150) NOT NULL,
    [Slug] nvarchar(150) NOT NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_Category_StatusCode] DEFAULT ('active'),
    [DisplaySequence] int NOT NULL CONSTRAINT [DF_Category_DisplaySequence] DEFAULT (0),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Category_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Category_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Category_CategoryId] UNIQUE NONCLUSTERED ([CategoryId]),
    CONSTRAINT [CK_Category_BusinessId] CHECK (LEN(TRIM([CategoryId])) > 0),
    CONSTRAINT [CK_Category_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_Category_ParentCategoryRowId] FOREIGN KEY ([ParentCategoryRowId]) REFERENCES [AppDbo].[Category] ([RowId]),
    CONSTRAINT [CK_Category_Rule1] CHECK (ParentCategoryRowId IS NULL OR ParentCategoryRowId <> RowId),
    CONSTRAINT [CK_Category_Rule2] CHECK (StatusCode IN ('active','inactive'))
);
