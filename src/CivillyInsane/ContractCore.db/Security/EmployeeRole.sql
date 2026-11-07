CREATE TABLE [AppDbo].[EmployeeRole]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [EmployeeRoleId] nvarchar(50) NOT NULL,
    [EmployeeEmail] nvarchar(320) NOT NULL,
    [RoleCode] varchar(20) NOT NULL,
    [SiteRowId] bigint NULL,
    [CreatedBy] nvarchar(320) NOT NULL,
    [RevokedUtc] datetime2(3) NULL,
    [RevokedBy] nvarchar(320) NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_EmployeeRole_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_EmployeeRole_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_EmployeeRole] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_EmployeeRole_EmployeeRoleId] UNIQUE NONCLUSTERED ([EmployeeRoleId]),
    CONSTRAINT [CK_EmployeeRole_BusinessId] CHECK (LEN(TRIM([EmployeeRoleId])) > 0),
    CONSTRAINT [CK_EmployeeRole_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_EmployeeRole_SiteRowId] FOREIGN KEY ([SiteRowId]) REFERENCES [AppDbo].[Site] ([RowId]),
    CONSTRAINT [CK_EmployeeRole_Rule1] CHECK (RoleCode IN ('owner','contributor','read-only')),
    CONSTRAINT [CK_EmployeeRole_Rule2] CHECK (EmployeeEmail COLLATE Latin1_General_100_BIN2 = LOWER(TRIM(EmployeeEmail)) COLLATE Latin1_General_100_BIN2)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeRole_1] ON [AppDbo].[EmployeeRole] (EmployeeEmail,RoleCode,SiteRowId) WHERE RevokedUtc IS NULL;
GO
