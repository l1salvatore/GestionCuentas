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
CREATE TABLE [Accounts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Alias] nvarchar(50) NULL,
    [Balance] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
);

CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [AccountId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Type] int NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Transactions_AccountId] ON [Transactions] ([AccountId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260213231557_InitialCreate', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Accounts]') AND [c].[name] = N'Alias');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Accounts] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Accounts] DROP COLUMN [Alias];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260214220928_RemoveAlias', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Accounts] ADD [RowVersion] rowversion NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260215131145_AddConcurrencyToken', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE UNIQUE INDEX [IX_Accounts_UserId] ON [Accounts] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260217171857_AddUniqueUserIdToAccounts', N'10.0.3');

COMMIT;
GO

