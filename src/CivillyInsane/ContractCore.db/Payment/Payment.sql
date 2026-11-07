CREATE TABLE [AppDbo].[Payment]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [PaymentId] nvarchar(50) NOT NULL,
    [SalesOrderRowId] bigint NOT NULL,
    [ProviderCode] nvarchar(50) NOT NULL,
    [ProviderPaymentId] nvarchar(200) NULL,
    [StatusCode] varchar(25) NOT NULL CONSTRAINT [DF_Payment_StatusCode] DEFAULT ('pending'),
    [AmountRequested] decimal(19,4) NOT NULL,
    [AmountAuthorized] decimal(19,4) NOT NULL CONSTRAINT [DF_Payment_AmountAuthorized] DEFAULT (0),
    [AmountCaptured] decimal(19,4) NOT NULL CONSTRAINT [DF_Payment_AmountCaptured] DEFAULT (0),
    [AmountRefunded] decimal(19,4) NOT NULL CONSTRAINT [DF_Payment_AmountRefunded] DEFAULT (0),
    [CurrencyCode] char(3) NOT NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Payment_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_Payment_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_Payment_PaymentId] UNIQUE NONCLUSTERED ([PaymentId]),
    CONSTRAINT [CK_Payment_BusinessId] CHECK (LEN(TRIM([PaymentId])) > 0),
    CONSTRAINT [CK_Payment_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_Payment_SalesOrderRowId] FOREIGN KEY ([SalesOrderRowId]) REFERENCES [AppDbo].[SalesOrder] ([RowId]),
    CONSTRAINT [FK_Payment_CurrencyCode] FOREIGN KEY ([CurrencyCode]) REFERENCES [AppDbo].[Currency] ([CurrencyCode]),
    CONSTRAINT [CK_Payment_Rule1] CHECK (AmountRequested >= 0 AND AmountAuthorized >= 0 AND AmountCaptured >= 0 AND AmountRefunded >= 0),
    CONSTRAINT [CK_Payment_Rule2] CHECK (AmountRefunded <= AmountCaptured AND AmountCaptured <= AmountRequested),
    CONSTRAINT [CK_Payment_Rule3] CHECK (StatusCode IN ('pending','requires-action','authorized','paid','partially-refunded','refunded','failed','voided','disputed'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Payment_1] ON [AppDbo].[Payment] (SalesOrderRowId);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Payment_2] ON [AppDbo].[Payment] (ProviderCode,ProviderPaymentId) WHERE ProviderPaymentId IS NOT NULL;
GO
