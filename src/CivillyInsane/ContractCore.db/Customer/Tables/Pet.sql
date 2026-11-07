CREATE TABLE [AppDbo].[Pet]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [PetId] nvarchar(50) NOT NULL,
    [CustomerRowId] bigint NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [SpeciesCode] varchar(20) NOT NULL,
    [BirthDate] date NULL,
    [Breed] nvarchar(100) NULL,
    [Weight] decimal(12,4) NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_Pet_StatusCode] DEFAULT ('active'),
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Pet_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Pet_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Pet] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Pet_PetId] UNIQUE NONCLUSTERED ([PetId]),
    CONSTRAINT [CK_Pet_BusinessId] CHECK (LEN(TRIM([PetId])) > 0),
    CONSTRAINT [CK_Pet_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_Pet_CustomerRowId] FOREIGN KEY ([CustomerRowId]) REFERENCES [AppDbo].[Customer] ([RowId]),
    CONSTRAINT [CK_Pet_Rule1] CHECK (SpeciesCode IN ('cat','dog')),
    CONSTRAINT [CK_Pet_Rule2] CHECK (Weight IS NULL OR Weight > 0),
    CONSTRAINT [CK_Pet_Rule3] CHECK (StatusCode IN ('active','archived'))
);
