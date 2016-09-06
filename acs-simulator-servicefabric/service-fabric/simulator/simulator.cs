using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace simulator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class simulator : StatelessService
    {
        public simulator(StatelessServiceContext context)
            : base(context)
        { }

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
            // Gather environment variables and connect to Azure SB queue
            string AZURE_SB_SERVICE_NAMESPACE = "acslogging";
            string AZURE_SB_SHARED_ACCESS_KEY_NAME = "RootManageSharedAccessKey";
            string AZURE_SB_SHARED_ACCESS_KEY = "gnLZ2ixKkXng7rNvaCbgl9ucxsEKK7vuD5QkLl1iemM=";
            string sbEndpoint = "Endpoint=sb://" + AZURE_SB_SERVICE_NAMESPACE + ".servicebus.windows.net/;SharedAccessKeyName=" + AZURE_SB_SHARED_ACCESS_KEY_NAME + ";SharedAccessKey=" + AZURE_SB_SHARED_ACCESS_KEY;

            long iterations = 0;

            QueueClient Client = QueueClient.CreateFromConnectionString(sbEndpoint, "statistics");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate load on SB Queue
                string msgText = "Azure Service Fabric simulator update number: " + System.Convert.ToString(iterations);

                //Randomly create data for SB Queue
                Random rnd = new Random();
                int r = rnd.Next(1, 11);
                string deviceid = "mydevice" + System.Convert.ToString(r);
                string temp = "";
                string pressure = "";
                string humidity = "";
                string windspeed = "";
                switch (r)
                {
                    case 1:
                        temp = "40.8";
                        pressure = "61.93";
                        humidity = "20";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 2:
                        temp = "50.8";
                        pressure = "62.83";
                        humidity = "30";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 3:
                        temp = "60.8";
                        pressure = "63.73";
                        humidity = "40";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 4:
                        temp = "70.8";
                        pressure = "64.63";
                        humidity = "50";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 5:
                        temp = "80.8";
                        pressure = "51.93";
                        humidity = "60";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 6:
                        temp = "90.8";
                        pressure = "55.93";
                        humidity = "70";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 7:
                        temp = "45.2";
                        pressure = "34.99";
                        humidity = "45";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 8:
                        temp = "55.2";
                        pressure = "38.99";
                        humidity = "55";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    case 9:
                        temp = "65.2";
                        pressure = "36.99";
                        humidity = "65";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                    default:
                        temp = "75.2";
                        pressure = "71.77";
                        humidity = "38";
                        windspeed = System.Convert.ToString(r) + ".5";
                        break;
                }

                BrokeredMessage message = new BrokeredMessage();
                message.Label = msgText;
                message.Properties.Add("deviceid", deviceid);
                message.Properties.Add("temp", temp);
                message.Properties.Add("pressure", pressure);
                message.Properties.Add("humidity", humidity);
                message.Properties.Add("windspeed", windspeed);

                Client.Send(message);

                ServiceEventSource.Current.ServiceMessage(this, "SF App iteration complete: " + msgText);

                iterations = iterations + 1;

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }
}
