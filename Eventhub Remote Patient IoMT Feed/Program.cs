using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;


namespace PatientIoTMFeed
{
    public class Program
    {
        private static EventHubClient eventHubClient;
        // Provide your Eventhub Connection strings below. 
        private const string EventHubConnectionString = "Endpoint=sb://rmustieventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=K4YEh1cPOHLk=";
        private const string EventHubName = "patient_iotm_eventhub";
        private static bool SetRandomPartitionKey = false;

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            // Creates an EventHubsConnectionStringBuilder object from a the connection string, and sets the EntityPath.
            // Typically the connection string should have the Entity Path in it, but for the sake of this simple scenario
            // we are using the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await SendMessagesToEventHub(1000);

            await eventHubClient.CloseAsync();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        // Creates an Event Hub client and sends 100 messages to the event hub.
        private static async Task SendMessagesToEventHub(int numMessagesToSend)
        {
            var rnd = new Random();
            
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    //Create Data
                    //var message = $"Message {i}";
                    var patient_id = rnd.Next(2000, 2999);
                    var body_temperature = rnd.NextDouble() * (105.9 - 95.0) + 95.0;
                    var heart_rate = rnd.Next(60,180 );
                    var spo_2 = rnd.Next(93, 99);
                    var systolic_Pressure = rnd.Next(96, 190);
                    var diastolic_Pressure = rnd.Next(40, 95);
                    var metric = new Metric { patientId = patient_id, timeStamp = DateTime.Now, bodyTemperature = Math.Round(body_temperature,1),heartRate = heart_rate, spo2 = spo_2 , systolicPressure =systolic_Pressure ,diastolicPressure = diastolic_Pressure};
                    //var jsonmessage = $"Message {i}, Some big payload: {Guid.NewGuid()}, Time: {DateTime.Now}";
                    var message = JsonConvert.SerializeObject(metric);

                    // Set random partition key?
                    if (SetRandomPartitionKey)
                    {
                        var pKey = Guid.NewGuid().ToString();
                        await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)), pKey);
                        Console.WriteLine($"Sent message '{i}':  '{message}' Partition Key: '{pKey}'");
                    }
                    else
                    {
                        await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                        Console.WriteLine($"Sent message '{i}': '{message}'");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                }

                await Task.Delay(1);
            }

            Console.WriteLine($"{numMessagesToSend} messages sent.");
        }

        private class Metric
        {
            public int patientId { get; set; }
            public DateTime timeStamp { get; set; }
            public Double bodyTemperature { get; set; }
            public int heartRate { get; set; }
            public int spo2 { get; set; }
            public int systolicPressure { get; set; }
            public int diastolicPressure { get; set; }
        }
    }
}
