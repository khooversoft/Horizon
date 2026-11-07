CREATE TABLE [AppDbo].[UnitOfMeasure]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [UnitOfMeasureId] nvarchar(50) NOT NULL,
    [UnitCode] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [DimensionCode] varchar(30) NOT NULL,
    [BaseUnitCode] nvarchar(20) NULL,
    [ConversionFactor] decimal(19,8) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_UnitOfMeasure_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_UnitOfMeasure_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_UnitOfMeasure] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_UnitOfMeasure_UnitOfMeasureId] UNIQUE NONCLUSTERED ([UnitOfMeasureId]),
    CONSTRAINT [CK_UnitOfMeasure_BusinessId] CHECK (LEN(TRIM([UnitOfMeasureId])) > 0),
    CONSTRAINT [CK_UnitOfMeasure_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_UnitOfMeasure_Rule1] CHECK (ConversionFactor IS NULL OR ConversionFactor > 0)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_UnitOfMeasure_1] ON [AppDbo].[UnitOfMeasure] (UnitCode);
GO
