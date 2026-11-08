
param location string = resourceGroup().location
param applicationName string

resource appIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: '${applicationName}-identity'
  location: location
}

output identityName string = appIdentity.name
output identityId string = appIdentity.id
output identityPrincipalId string = appIdentity.properties.principalId
output identityClientId string = appIdentity.properties.clientId
