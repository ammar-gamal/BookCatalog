param location string = resourceGroup().location

@description('Globally Unique name for the SQL Server instance.')
param sqlServerName string = 'sqlserver-${uniqueString(resourceGroup().id)}'

@description('Name of the application')
param applicationName string

// my deployment will use group,the purpose of using a group is to allow me(my account itself) and the pipeline to be the admins
// and can access db,  the pipeline is need to be admin because it want to execute migrations
@description('Principle type for Entra Admin of the Azure SQL Database')
@allowed([
  'User'
  'Group'
  'Application'
])
param sqlServerPrincipleType string

@description('Login name that will be the Entra Admin To Azure SQL Database')
param sqlServerAdminLogin string

@description('ObjectId will be the Entra Admin To Azure SQL Database')
param sqlServerAdminObjectId string 



resource sqlServer 'Microsoft.Sql/servers@2025-01-01' = {
  name: sqlServerName
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      tenantId: tenant().tenantId
      principalType: sqlServerPrincipleType
      login: sqlServerAdminLogin
      sid: sqlServerAdminObjectId
    }
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}
resource sqlFirewall 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  // name: 'AllowAll"'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2025-01-01' = {
  parent: sqlServer
  name: '${applicationName}-db'
  location: location
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
  }
  properties: {
    freeLimitExhaustionBehavior: 'AutoPause'
    useFreeLimit: true
  }
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
output connectionString string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${database.name};Authentication=Active Directory Default;'
