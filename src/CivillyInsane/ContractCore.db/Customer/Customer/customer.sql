CREATE TABLE [AppDbo].[customer]
(
    [customer_id] bigint IDENTITY(1,1) NOT NULL,
    [email] nvarchar(320) NOT NULL,
    [first_name] nvarchar(100) NULL,
    [last_name] nvarchar(100) NULL,
    [phone] varchar(30) NULL,
    [created_utc] datetime2(3) NOT NULL CONSTRAINT [DF_customer_created_utc] DEFAULT (SYSUTCDATETIME()),
    [modified_utc] datetime2(3) NOT NULL CONSTRAINT [DF_customer_modified_utc] DEFAULT (SYSUTCDATETIME()),
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_customer] PRIMARY KEY CLUSTERED ([customer_id]),
    CONSTRAINT [UQ_customer_email] UNIQUE NONCLUSTERED ([email]),
    CONSTRAINT [CK_customer_email] CHECK ([email] LIKE '%_@_%'),
    CONSTRAINT [CK_customer_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)
);
GO
