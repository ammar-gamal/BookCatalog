
:ON ERROR EXIT

SET NOCOUNT ON;

PRINT '=== Grant managed identity permissions ===';
PRINT 'Target identity: $(IdentityName)';
PRINT 'Target database: ' + DB_NAME();

PRINT '=== Step 1: Check whether user already exists ===';

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = N'$(IdentityName)'
)
BEGIN
    PRINT 'User does not exist. Creating database user...';

    CREATE USER [$(IdentityName)] FROM EXTERNAL PROVIDER;

    PRINT 'CREATE USER completed.';
END
ELSE
BEGIN
    PRINT 'User already exists.';
END;

PRINT '=== Step 2: Grant database roles ===';

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals rolep
        ON rolep.principal_id = drm.role_principal_id
    JOIN sys.database_principals member
        ON member.principal_id = drm.member_principal_id
    WHERE rolep.name = N'db_datareader'
      AND member.name = N'$(IdentityName)'
)
BEGIN
    ALTER ROLE db_datareader ADD MEMBER [$(IdentityName)];
    PRINT 'Granted db_datareader.';
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals rolep
        ON rolep.principal_id = drm.role_principal_id
    JOIN sys.database_principals member
        ON member.principal_id = drm.member_principal_id
    WHERE rolep.name = N'db_datawriter'
      AND member.name = N'$(IdentityName)'
)
BEGIN
    ALTER ROLE db_datawriter ADD MEMBER [$(IdentityName)];
    PRINT 'Granted db_datawriter.';
END;

PRINT '=== Step 3: Verify final permissions ===';

SELECT
    member.name AS IdentityName,
    rolep.name AS DatabaseRole
FROM sys.database_role_members drm
JOIN sys.database_principals rolep
    ON rolep.principal_id = drm.role_principal_id
JOIN sys.database_principals member
    ON member.principal_id = drm.member_principal_id
WHERE member.name = N'$(IdentityName)';

PRINT '=== Grant script completed successfully ===';
