CREATE TABLE [AppDbo].[SalesOrderAddress]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SalesOrderAddressId] nvarchar(50) NOT NULL,
    [SalesOrderRowId] bigint NOT NULL,
    [AddressTypeCode] varchar(20) NOT NULL,
    [RecipientName] nvarchar(200) NOT NULL,
    [CompanyName] nvarchar(200) NULL,
    [AddressLine1] nvarchar(200) NOT NULL,
    [AddressLine2] nvarchar(200) NULL,
    [City] nvarchar(100) NOT NULL,
    [RegionCode] nvarchar(100) NULL,
    [PostalCode] nvarchar(30) NOT NULL,
    [CountryCode] char(2) NOT NULL,
    [Phone] nvarchar(50) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SalesOrderAddress_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SalesOrderAddress] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SalesOrderAddress_SalesOrderAddressId] UNIQUE NONCLUSTERED ([SalesOrderAddressId]),
    CONSTRAINT [CK_SalesOrderAddress_BusinessId] CHECK (LEN(TRIM([SalesOrderAddressId])) > 0),
    CONSTRAINT [CK_SalesOrderAddress_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SalesOrderAddress_SalesOrderRowId] FOREIGN KEY ([SalesOrderRowId]) REFERENCES [AppDbo].[SalesOrder] ([RowId]),
    CONSTRAINT [FK_SalesOrderAddress_CountryCode] FOREIGN KEY ([CountryCode]) REFERENCES [AppDbo].[Country] ([CountryCode]),
    CONSTRAINT [CK_SalesOrderAddress_Rule1] CHECK (AddressTypeCode IN ('shipping','billing'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SalesOrderAddress_1] ON [AppDbo].[SalesOrderAddress] (SalesOrderRowId,AddressTypeCode);
GO
