param location string = resourceGroup().location
param connectionString string
param apiImage string
param applicationName string
param applicationManagedIdentityId string

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
    // type: 'SystemAssigned'
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
