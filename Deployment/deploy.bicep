param location string = 'centralus'
param appEnvironment string
param branch string
param clientId string
param adInstance string
param managedUserId string
param prefix string
param scriptVersion string = utcNow()
param version string

var demoName = '${prefix}demo'
var tags = {
  'stack-name': demoName
  'stack-environment': appEnvironment
  'stack-version': version
  'stack-branch': branch
}

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: demoName
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: demoName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
  tags: tags
}

resource hostingPlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: demoName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  tags: tags
}

var storageConnection = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'

resource funcapp 'Microsoft.Web/sites@2020-12-01' = {
  name: demoName
  location: location
  tags: tags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: hostingPlan.id
    clientAffinityEnabled: true
    siteConfig: {
      netFrameworkVersion: 'v6.0'
      webSocketsEnabled: true
      appSettings: [
        {
          'name': 'APPINSIGHTS_INSTRUMENTATIONKEY'
          'value': appInsights.properties.InstrumentationKey
        }
        {
          'name': 'AzureAd:ClientId'
          'value': clientId
        }
        {
          'name': 'AzureAd:Instance'
          'value': adInstance
        }
        {
          'name': 'TableStorageConnection'
          'value': storageConnection
        }
        {
          'name': 'AzureWebJobsStorage'
          'value': storageConnection
        }
        {
          'name': 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          'value': storageConnection
        }
        {
          'name': 'WEBSITE_CONTENTSHARE'
          'value': 'democontent'
        }
        {
          'name': 'FUNCTIONS_WORKER_RUNTIME'
          'value': 'dotnet-isolated'
        }
        {
          'name': 'FUNCTIONS_EXTENSION_VERSION'
          'value': '~3'
        }
        {
          'name': 'ApplicationInsightsAgent_EXTENSION_VERSION'
          'value': '~2'
        }
        {
          'name': 'XDT_MicrosoftApplicationInsights_Mode'
          'value': 'default'
        }
        {
          'name': 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          'value': appInsights.properties.ConnectionString
        }
      ]
    }
  }
}

resource apiSite 'Microsoft.Web/sites/config@2021-01-15' = {
  parent: funcapp
  name: 'web'
  properties: {
    cors: {
      allowedOrigins: [
        staticWebsiteSetup.properties.outputs.endpoint
        edgeUrl
      ]
    }
  }
}

resource cdn 'Microsoft.Cdn/profiles@2020-09-01' = {
  name: demoName
  location: location
  tags: tags
  sku: {
    name: 'Standard_Microsoft'
  }
}

resource staticWebsiteSetup 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: demoName
  kind: 'AzurePowerShell'
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${subscription().id}/resourceGroups/${resourceGroup().name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${managedUserId}': {}
    }
  }
  properties: {
    forceUpdateTag: scriptVersion
    azPowerShellVersion: '5.0'
    retentionInterval: 'P1D'
    arguments: '-StorageAccountName ${storageAccount.name} -ResourceGroupName ${resourceGroup().name}'
    scriptContent: loadTextContent('deploywebsite.ps1')
  }
}

var edgeUrl = 'https://${demoName}.azureedge.net'

var websiteUrl = replace(replace(staticWebsiteSetup.properties.outputs.endpoint, 'https://', ''), '/', '')
resource cdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2020-09-01' = {
  name: demoName
  parent: cdn
  location: 'global'
  tags: tags
  properties: {
    originHostHeader: websiteUrl
    isCompressionEnabled: true
    isHttpAllowed: false
    isHttpsAllowed: true
    contentTypesToCompress: [
      'application/eot'
      'application/font'
      'application/font-sfnt'
      'application/javascript'
      'application/json'
      'application/opentype'
      'application/otf'
      'application/pkcs7-mime'
      'application/truetype'
      'application/ttf'
      'application/vnd.ms-fontobject'
      'application/xhtml+xml'
      'application/xml'
      'application/xml+rss'
      'application/x-font-opentype'
      'application/x-font-truetype'
      'application/x-font-ttf'
      'application/x-httpd-cgi'
      'application/x-javascript'
      'application/x-mpegurl'
      'application/x-opentype'
      'application/x-otf'
      'application/x-perl'
      'application/x-ttf'
      'font/eot'
      'font/ttf'
      'font/otf'
      'font/opentype'
      'image/svg+xml'
      'text/css'
      'text/csv'
      'text/html'
      'text/javascript'
      'text/js'
      'text/plain'
      'text/richtext'
      'text/tab-separated-values'
      'text/xml'
      'text/x-script'
      'text/x-component'
      'text/x-java-source'
    ]
    origins: [
      {
        name: replace(websiteUrl, '.', '-')
        properties: {
          hostName: websiteUrl
          httpPort: 80
          httpsPort: 443
          originHostHeader: websiteUrl
          priority: 1
          weight: 1000
          enabled: true
        }
      }
    ]
  }
}

resource storageAccountBlobServices 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            staticWebsiteSetup.properties.outputs.endpoint
          ]
          allowedMethods: [
            'POST'
            'GET'
            'OPTIONS'
            'HEAD'
            'PUT'
            'MERGE'
            'DELETE'
          ]
          maxAgeInSeconds: 120
          exposedHeaders: [
            '*'
          ]
          allowedHeaders: [
            '*'
          ]
        }
        {
          allowedOrigins: [
            edgeUrl
          ]
          allowedMethods: [
            'POST'
            'GET'
            'OPTIONS'
            'HEAD'
            'PUT'
            'MERGE'
            'DELETE'
          ]
          maxAgeInSeconds: 120
          exposedHeaders: [
            '*'
          ]
          allowedHeaders: [
            '*'
          ]
        }
      ]
    }
  }
}

output accountName string = storageAccount.name
output funcName string = funcapp.name
