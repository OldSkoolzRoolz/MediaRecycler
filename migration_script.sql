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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621235639_postype'
)
BEGIN
    CREATE TABLE [PostPages] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [Link] nvarchar(150) NOT NULL,
        [IsProcessed] bit NOT NULL,
        CONSTRAINT [PK__PostPage__3214EC07B0094615] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621235639_postype'
)
BEGIN
    CREATE TABLE [Posts] (
        [Id] int NOT NULL IDENTITY,
        [PostId] nvarchar(50) NOT NULL,
        [Link] nvarchar(150) NOT NULL,
        [IsDownloaded] bit NOT NULL,
        CONSTRAINT [PK__Table__3214EC070FFF6B39] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621235639_postype'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250621235639_postype', N'9.0.6');
END;

COMMIT;
GO

