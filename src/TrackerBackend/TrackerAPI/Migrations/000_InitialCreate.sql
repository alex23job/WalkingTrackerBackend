CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808215051_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "EmailHash" text NOT NULL,
        "PasswordHash" text NOT NULL,
        "StepLengthMeters" double precision NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808215051_InitialCreate') THEN
    CREATE TABLE "TrainingSessions" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "StartTime" timestamp with time zone NOT NULL,
        "EndTime" timestamp with time zone,
        "DistanceMeters" double precision NOT NULL,
        "RouteGeometryWkt" text,
        CONSTRAINT "PK_TrainingSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrainingSessions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808215051_InitialCreate') THEN
    CREATE INDEX "IX_TrainingSessions_UserId" ON "TrainingSessions" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808215051_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260808215051_InitialCreate', '9.0.18');
    END IF;
END $EF$;
COMMIT;