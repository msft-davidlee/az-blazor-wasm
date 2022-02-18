# Disclaimer
The information contained in this README.md file and any accompanying materials (including, but not limited to, scripts, sample codes, etc.) are provided "AS-IS" and "WITH ALL FAULTS." Any estimated pricing information is provided solely for demonstration purposes and does not represent final pricing and Microsoft assumes no liability arising from your use of the information. Microsoft makes NO GUARANTEES OR WARRANTIES OF ANY KIND, WHETHER EXPRESSED OR IMPLIED, in providing this information, including any pricing information.

# Introduction
This project demonstrates a Blazor WASM hosted on an Azure Storage Static Website with a backend API hosted as an HTTP triggered Azure Function.

### Azure Function
The Azure Function itself is running in .NET 5, and is using the isolated process. For more information on the difference, see: https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide.

## Architecture
1. Azure B2C which provides a platform to sign up and authenticate users. If a user forgets his/her password, it is all taken care of by Azure B2C and we don't have do anything there.
2. The API is a HTTP Triggered Azure Function. Azure Function will scale based on demand and we don't have to worry about this. The cost for running Azure Functions in consumption mode is relatively inexpensive with a few dollars for millions of executions.
3. The API is protected by Azure B2C via OAuth 2.0 authorization code flow.
4. The database is an Azure Table Storage which is great for static workloads which don't change often. There are plenty of power for inexpensive reads with the right partitioning strategy.
5. The frontend is a Blazor WASM app that is downloaded directly to and executes within the context of the user browser.
6. The frontend is hosted as a Static Web App on Storage account which makes it an inexpensive option because we are just serving static content and don't need the compute power such as App Service. 
7. The frontend content is served via Azure CDN which makes it extremely performant to use and we don't really have to worry about making sure it is globally available fast. The cost is also cents per GB, making it relatively inexpensive for the size of the Blazor WASM app we are serving.
8. CORS needs to be configured for the API backend in order for the Blazor WASM app to hit the endpoint which is really the host URL.
9. CI/CD is taken care of with GitHub Actions. The deployment pipeline is configured so that each environment will have its specific configurations via GitHub secrets, which makes it flexible and easily administred.

![Architecture](/Architecture/Solution.png)

## Considerations
1. The API can be seperated from this project as an independent microservice with its own deployment so we can push out features or fixes faster. We do have to make the frontend aware by introducing the API versioning concept. 
2. We can enhance the API with either Azure Frontdoor or Application Gateway and enable the Web Application Firewall (WAF) feature.
3. We can apply a free custom domain name on Azure CDN where SSL certificate is applied and rotated for free.
4. Instead of storing the connection string on the configuration of Azure Functions, we can create a managed identity for the Azure Function and assign the appropriate RBAC role.

# Get Started
To create this, use your personal Azure subscription with AAD that you control - as well as a B2C instance, please follow the steps below. 

1. Fork this git repo. See: https://docs.github.com/en/get-started/quickstart/fork-a-repo
2. Create two resource groups to represent two environments. Suffix each resource group name with either a -dev or -prod. An example could be todo-dev and todo-prod.
3. Next, you must create a service principal with Contributor roles assigned to the two resource groups.
4. In your github organization for your project, create two environments, and named them dev and prod respectively.
5. Create the following secrets in your github per environment. Be sure to populate with your desired values. The values below are all suggestions.
6. Note that the environment suffix of dev or prod will be appened to your resource group but you will have the option to define your own resource prefix.
7. Create user flow in your B2C tenant and be sure your App Registration include the appropriate Urls. See Secrets below.

## Secrets
| Name | Comments |
| --- | --- |
| AZURE_CREDENTIALS | <pre>{<br/>&nbsp;&nbsp;&nbsp;&nbsp;"clientId": "",<br/>&nbsp;&nbsp;&nbsp;&nbsp;"clientSecret": "", <br/>&nbsp;&nbsp;&nbsp;&nbsp;"subscriptionId": "",<br/>&nbsp;&nbsp;&nbsp;&nbsp;"tenantId": "" <br/>}</pre> |
| PREFIX | mytodos - or whatever name you would like for all your resources |
| RESOURCE_GROUP | todo - or whatever name you give to the resource group |
| CLI_AUTHORITY | Authority of the Blazor WASP app |
| CLI_CLIENT_ID | Client Id of the Blazor WASM app |
| CLI_SCOPE | The App Id found in the API app registration that exposes this API |
| API_AD_INSTANCE | AD instance found in your B2C tenant |
| API_CLIENT_ID | Cient Id of the API found in your app registration in your B2C tenant |
| SP_ID | user id you have assigned as a managed user identity in your resource group |

## Docker Support
This project supports the use of Docker for the Azure Functions API. If you have docker desktop on your local environment, you can also run the Azure Function as a container instance.

## Authentication
For the purpose of testing without authentication enabled, you can add a configuation like so in your API Configurations. This is recommanded only if you are just testing the API directly without a user.

```
"DisableAuthentication": "true"
```

An example of invoking an API locally with this option. Notice we no longer require a Header with a bearer token. The user will be unknownuser@contoso.com.

```
$body = @{ description=$Description; }
$url = "http://localhost:7071/todo"

Invoke-RestMethod -UseBasicParsing -Uri $url -Body ($body | ConvertTo-Json) -Method Post
```

## Have an issue?
You are welcome to create an issue if you need help but please note that there is no timeline to answer or resolve any issues you have with the contents of this project. Use the contents of this project at your own risk!