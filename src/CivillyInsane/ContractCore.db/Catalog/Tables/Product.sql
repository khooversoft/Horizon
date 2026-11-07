CREATE TABLE [AppDbo].[Product]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [ProductId] nvarchar(50) NOT NULL,
    [Name] nvarchar(250) NOT NULL,
    [ShortDescription] nvarchar(500) NULL,
    [Description] nvarchar(max) NULL,
    [ProductTypeCode] varchar(30) NOT NULL,
    [StatusCode] varchar(20) NOT NULL CONSTRAINT [DF_Product_StatusCode] DEFAULT ('draft'),
    [SubscriptionEligible] bit NOT NULL CONSTRAINT [DF_Product_SubscriptionEligible] DEFAULT (0),
    [DefaultFulfillmentProviderCode] nvarchar(50) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Product_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Product_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Product_ProductId] UNIQUE NONCLUSTERED ([ProductId]),
    CONSTRAINT [CK_Product_BusinessId] CHECK (LEN(TRIM([ProductId])) > 0),
    CONSTRAINT [CK_Product_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [CK_Product_Rule1] CHECK (ProductTypeCode IN ('physical','service')),
    CONSTRAINT [CK_Product_Rule2] CHECK (StatusCode IN ('draft','active','inactive','retired'))
);
