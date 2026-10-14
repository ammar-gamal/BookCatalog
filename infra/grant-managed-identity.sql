IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = '$(IdentityName)'
)
BEGIN
    CREATE USER [$(IdentityName)] FROM EXTERNAL PROVIDER;
END

ALTER ROLE db_datareader ADD MEMBER [$(IdentityName)];
ALTER ROLE db_datawriter ADD MEMBER [$(IdentityName)];