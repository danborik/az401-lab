// Lab 3.9: App Service (Linux, S1 so deployment slots are available) + staging slot.
// Deploy:  az deployment group create -g rg-az401-lab -f infra/main.bicep
// Cost: S1 is billed per hour - delete or scale down after the lab.
param location string = resourceGroup().location
param name string = 'az401-${uniqueString(resourceGroup().id)}'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${name}-plan'
  location: location
  sku: { name: 'S1' }
  kind: 'linux'
  properties: { reserved: true }
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  identity: { type: 'SystemAssigned' } // used for App Configuration / Key Vault labs
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      healthCheckPath: '/health'
      appSettings: [
        { name: 'Settings__Message', value: 'Hello from production slot' }
      ]
    }
  }
}

resource staging 'Microsoft.Web/sites/slots@2023-12-01' = {
  parent: app
  name: 'staging'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      healthCheckPath: '/health'
      appSettings: [
        { name: 'Settings__Message', value: 'Hello from staging slot' }
      ]
    }
  }
}

output appName string = app.name
output appUrl string = 'https://${app.properties.defaultHostName}'
output stagingUrl string = 'https://${staging.properties.defaultHostName}'
