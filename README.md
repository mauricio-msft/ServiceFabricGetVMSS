# Get Azure ScaleSets per Service Fabric Cluster Console Application

## Create a Service Principal
The sample assumes that you have already set up a [Service Principal](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?toc=%2Fazure%2Fazure-resource-manager%2Ftoc.json&view=azure-cli-latest) to access your Azure subscription.

Required information:
+ Subscription
+ AzureTenantId
+ AzureClientId
+ AzureClientSecret

Can be obtained from the previous step. You already have the AzureClientSecret since you used it to create the Service Principal. The AzureTenantId and the AzureClientId will be returned by the az ad sp create-for-rbac command you executed before.

Update the file appsettings.json with all required info.

## Request the Access Token
Next, obtain an authentication token for your Service Principal.
```
string token = await AuthenticationHelpers.AcquireTokenBySPN(
	tenantId, clientId, clientSecret);
```

## Call the Azure ARM API
Then, gets the list of Service Fabric cluster resources created in the specified subscription calling the Azure ARM API using plain REST.
[List Request](https://docs.microsoft.com/en-us/rest/api/servicefabric/sfrp-api-clusters_list)

## Resource Graph query
And last, using Resource Graph you can get the VMSS details where you can associate with every cluster ID.
Resource: [Quickstart: Run your first Resource Graph query using .NET Core](https://docs.microsoft.com/en-us/azure/governance/resource-graph/first-query-dotnet)

## Dependencies
### Required Packages:
+ Microsoft.Azure.Management.ResourceGraph" Version="2.0.0"
+ Microsoft.Azure.Management.ResourceManager" Version="3.13.1-preview"
+ Microsoft.Extensions.Configuration" Version="5.0.0"
+ Microsoft.Extensions.Configuration.FileExtensions" Version="5.0.0"
+ Microsoft.Extensions.Configuration.Json" Version="5.0.0"
+ Microsoft.IdentityModel.Clients.ActiveDirectory" Version="5.2.9"
+ Microsoft.Rest.ClientRuntime" Version="2.3.23"
+ System.Net.Http.Formatting.Extension" Version="5.2.3"