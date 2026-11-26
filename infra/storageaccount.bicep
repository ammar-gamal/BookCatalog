param location string
param applicationName string
param principalId string

@description('Globally unique, 3-24 chars, lowercase letters and digits only')
param storageAccountName string = take(
  'st${toLower(replace(applicationName,'-',''))}${uniqueString(resourceGroup().id)}',
  24
)

var blobContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor
//Role is Resource created in the subcription scope,
//RoleId is a GUID that reporesent this role
//To assign a role to a principal, we need the roleDefinitionId, which is the full resource ID of the role.
//the full ResourceId of the role is /subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}

resource storageAccount 'Microsoft.Storage/storageAccounts@2026-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2026-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, principalId, blobContributorRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobContributorRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output storageAccountName string = storageAccount.name
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
