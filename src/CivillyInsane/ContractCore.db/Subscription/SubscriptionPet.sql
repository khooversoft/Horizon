CREATE TABLE [AppDbo].[SubscriptionPet]
(
    [RowId] bigint IDENTITY(1,1) NOT NULL,
    [SubscriptionPetId] nvarchar(50) NOT NULL,
    [SubscriptionRowId] bigint NOT NULL,
    [PetRowId] bigint NOT NULL,
    [CustomerRowId] bigint NOT NULL,
    [CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SubscriptionPet_CreatedUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_SubscriptionPet_ModifiedUtc] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] rowversion NOT NULL,
    [JsonData] varchar(max) NULL,
    CONSTRAINT [PK_SubscriptionPet] PRIMARY KEY CLUSTERED ([RowId]),
    CONSTRAINT [UQ_SubscriptionPet_SubscriptionPetId] UNIQUE NONCLUSTERED ([SubscriptionPetId]),
    CONSTRAINT [CK_SubscriptionPet_BusinessId] CHECK (LEN(TRIM([SubscriptionPetId])) > 0),
    CONSTRAINT [CK_SubscriptionPet_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1),
    CONSTRAINT [FK_SubscriptionPet_SubscriptionRowId] FOREIGN KEY ([SubscriptionRowId]) REFERENCES [AppDbo].[Subscription] ([RowId]),
    CONSTRAINT [FK_SubscriptionPet_PetRowId] FOREIGN KEY ([PetRowId]) REFERENCES [AppDbo].[Pet] ([RowId]),
    CONSTRAINT [FK_SubscriptionPet_CustomerRowId] FOREIGN KEY ([CustomerRowId]) REFERENCES [AppDbo].[Customer] ([RowId]),
    CONSTRAINT [FK_SubscriptionPet_Subscription_Ownership] FOREIGN KEY (SubscriptionRowId,CustomerRowId) REFERENCES [AppDbo].[Subscription] (RowId,CustomerRowId),
    CONSTRAINT [FK_SubscriptionPet_Pet_Ownership] FOREIGN KEY (PetRowId,CustomerRowId) REFERENCES [AppDbo].[Pet] (RowId,CustomerRowId)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SubscriptionPet_1] ON [AppDbo].[SubscriptionPet] (SubscriptionRowId,PetRowId);
GO
