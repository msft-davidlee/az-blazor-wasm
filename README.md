# Disclaimer
The information contained in this README.md file and any accompanying materials (including, but not limited to, scripts, sample codes, etc.) are provided "AS-IS" and "WITH ALL FAULTS." Any estimated pricing information is provided solely for demonstration purposes and does not represent final pricing and Microsoft assumes no liability arising from your use of the information. Microsoft makes NO GUARANTEES OR WARRANTIES OF ANY KIND, WHETHER EXPRESSED OR IMPLIED, in providing this information, including any pricing information.

# Introduction
This project demonstrates a Blazor WASM hosted on an Azure Storage Static Website with a backend API hosted as an HTTP triggered Azure Function. 

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
| LOCATION | location of this workload |
| SP_ID | user id you have assigned as a managed user identity in your resource group |
