using Azure.Identity;
using Azure.Messaging.ServiceBus;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var serviceBusFullyQualifiedName = Environment.GetEnvironmentVariable("ServiceBusFullyQualifiedName");

var serviceBusClient = new ServiceBusClient(serviceBusFullyQualifiedName,
    new DefaultAzureCredential(), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpTcp });

var options = new ServiceBusSessionProcessorOptions
{
    AutoCompleteMessages = false,
    MaxConcurrentSessions = 1,//One session per time
    MaxConcurrentCallsPerSession = 1,
    SessionIdleTimeout = TimeSpan.FromSeconds(1)
};
await using ServiceBusSessionProcessor processor = serviceBusClient.CreateSessionProcessor("my-queue", options);

processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

async Task MessageHandler(ProcessSessionMessageEventArgs args)
{
    var message = args.Message.Body.ToString();
    Console.WriteLine($"Received: {message}");

    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.Write("Error: "+ args.ToString());
    return Task.CompletedTask;
}

// start processing
await processor.StartProcessingAsync(cts.Token);

Console.WriteLine("Press any keys to exit");
Console.ReadLine();
