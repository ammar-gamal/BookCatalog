param location string = resourceGroup().location
param connectionString string
param apiImage string
param applicationName string
param applicationManagedIdentityId string
param applicationManagedIdentityPrincipalId string
param applicationManagedIdentityClientId string

module storage 'storage-account.bicep' = {
  name: 'storageaccount'
  params: {
    location: location
    applicationName: applicationName
    principalId: applicationManagedIdentityPrincipalId
  }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2026-03-01' = {
  name: '${applicationName}-logs'
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource managedEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: '${applicationName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().PrimarySharedKey
      }
    }
  }
}

resource api 'Microsoft.App/containerApps@2026-01-01' = {
  name: '${applicationName}-api'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${applicationManagedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: managedEnvironment.id

    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
    }

    template: {
      containers: [
        {
          name: 'api'
          image: apiImage

          resources: {
            cpu: any('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ConnectionStrings__Database'
              value: connectionString
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: applicationManagedIdentityClientId
            }
            {
              name: 'Azure__BlobService__EndPoint'
              value: storage.outputs.blobEndpoint
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
          ]
        }
      ]

      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
}
