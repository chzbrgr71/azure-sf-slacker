using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;

namespace slackpost
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class slackpost : StatelessService
    {
        public slackpost(StatelessServiceContext context)
            : base(context)
        { }
        static string GetIPAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress address in ipHostInfo.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            long iterations = 0;
            string urlWithAccessToken = "https://hooks.slack.com/services/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";
            string computerName = Environment.MachineName;
            string ipAddress = GetIPAddress();

            // Use random number to determine wait time for each iteration
            Random rnd = new Random();
            int r = rnd.Next(8, 15);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Gather values
                string svcVersion = Context.CodePackageActivationContext.GetServiceManifestVersion();
                var applicationName = new Uri("fabric:/azure_sf_slacker"); // or use Context.CodePackageActivationContext.ApplicationName
                var fabclient = new FabricClient();
                var applications = await fabclient.QueryManager.GetApplicationListAsync(applicationName).ConfigureAwait(false);
                var appVersion = applications[0].ApplicationTypeVersion;

                string slackMessage = computerName + "/" + ipAddress + " posting to Slack via SF app slackpost. App v" + appVersion + " Service v" + svcVersion;
                
                // Post to Slack
                SlackClient client = new SlackClient(urlWithAccessToken);

                client.PostMessage(username: "azure-service-fabric",
                           text: slackMessage,
                           channel: "#general");

                //Log event
                ServiceEventSource.Current.ServiceMessage(this, "slackpost SF app completed an iteration");

                iterations = iterations + 1;
                await Task.Delay(TimeSpan.FromSeconds(r), cancellationToken);
            }
        }
    }
}
public class SlackClient
{
    private readonly Uri _uri;
    private readonly Encoding _encoding = new UTF8Encoding();

    public SlackClient(string urlWithAccessToken)
    {
        _uri = new Uri(urlWithAccessToken);
    }

    //Post a message using simple strings
    public void PostMessage(string text, string username = null, string channel = null)
    {
        Payload payload = new Payload()
        {
            Channel = channel,
            Username = username,
            Text = text
        };

        PostMessage(payload);
    }

    //Post a message using a Payload object
    public void PostMessage(Payload payload)
    {
        string payloadJson = JsonConvert.SerializeObject(payload);

        using (WebClient client = new WebClient())
        {
            NameValueCollection data = new NameValueCollection();
            data["payload"] = payloadJson;

            var response = client.UploadValues(_uri, "POST", data);

            //The response text is usually "ok"
            string responseText = _encoding.GetString(response);
        }
    }
}

//This class serializes into the Json payload required by Slack Incoming WebHooks
public class Payload
{
    [JsonProperty("channel")]
    public string Channel { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}
