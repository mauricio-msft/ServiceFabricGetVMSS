using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Rest;

namespace HttpClientSample
{
    class Program
    {

        static string Subscription;

        static void Main(string[] args)
        {
            try
            {
                MainAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetBaseException().Message);
            }
        }

        static async Task MainAsync()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            Subscription = config["Subscription"];
            string tenantId = config["AzureTenantId"];
            string clientId = config["AzureClientId"];
            string clientSecret = config["AzureClientSecret"];

            string token = await AuthenticationHelpers.AcquireTokenBySPN(tenantId, clientId, clientSecret);

            //using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.BaseAddress = new Uri("https://management.azure.com/");

                await ListAllServiceFabricResourcesInSubscription(client, token);
            }
        }

        static async Task ListAllServiceFabricResourcesInSubscription(HttpClient client, string token)
        {
            JArray clusters = new JArray();
            string url = $"/subscriptions/{Subscription}/providers/Microsoft.ServiceFabric/clusters?api-version=2018-02-01";
            while (!String.IsNullOrEmpty(url))
            {
                using (var response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsAsync<dynamic>();
                    clusters.Merge(json.value);

                    // Follow the next page of results
                    url = json.nextLink;
                }
            }

            ServiceClientCredentials serviceClientCreds = new TokenCredentials(token);
            ResourceGraphClient argClient = new ResourceGraphClient(serviceClientCreds);
            QueryRequest request = new QueryRequest();
            request.Subscriptions = new List<string>(){ Subscription };
            request.Query = "resources " +
                " | where type =~ \"Microsoft.Compute/VirtualMachineScaleSets\" " +
                " | extend extensions = properties.virtualMachineProfile.extensionProfile.extensions " +
                " | mvexpand extensions " +
                " | where extensions.properties.type == \"ServiceFabricNode\" " +
                " | extend clusterId = split(extensions.properties.settings.clusterEndpoint, '/')[5] " +
                " | extend nodeTypeRef = extensions.properties.settings.nodeTypeRef " +
                " | project id, clusterId, nodeTypeRef";

            QueryResponse graphResponse = argClient.Resources(request);
            JArray results = (JArray)JObject.FromObject(graphResponse.Data)["rows"];

            foreach(var cluster in clusters)
            {
                Console.WriteLine("SF Cluster: {0}", cluster["name"]);
                Console.WriteLine("       vmss: ");

                foreach (JArray vmss in results)
                {
                    if(cluster["properties"]["clusterId"].Equals(vmss[1]))
                        Console.WriteLine("             {0} (vmssId -> {1})", vmss[2], vmss[0]);
                }

                Console.WriteLine("");
            }
        }      
    }
}