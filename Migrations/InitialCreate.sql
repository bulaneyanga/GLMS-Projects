IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Clients] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(160) NOT NULL,
    [ContactDetails] nvarchar(240) NOT NULL,
    [Region] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);

CREATE TABLE [Contracts] (
    [Id] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [ServiceLevel] nvarchar(300) NOT NULL,
    [AgreementFileName] nvarchar(500) NULL,
    [AgreementStorageKey] nvarchar(1000) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ServiceRequests] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [AmountUsd] decimal(18,2) NOT NULL,
    [CostZar] decimal(18,2) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Status] nvarchar(40) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);

CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261014093222_InitialCreate', N'10.0.13');

COMMIT;
GO

