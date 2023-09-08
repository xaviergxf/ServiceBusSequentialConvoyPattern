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
var sender = serviceBusClient.CreateSender("my-queue");


await Parallel.ForAsync(0, 10, cts.Token, async (i, cancellationToken) =>
{
    var sessionID = i.ToString();
    var orderCreatedMessage = $"Order {i} Created";
    var orderPayedMessage = $"Order {i} Payed";
    var orderShippedMessage = $"Order {i} Shipped";
    var orderDeliveredMessage = $"Order {i} Delivered";

    await sender.SendMessageAsync(new ServiceBusMessage(orderCreatedMessage) { SessionId = sessionID }, cancellationToken);
    Console.WriteLine($"Sent: {orderCreatedMessage}");
    await sender.SendMessageAsync(new ServiceBusMessage(orderPayedMessage) { SessionId = sessionID }, cancellationToken);
    Console.WriteLine($"Sent: {orderPayedMessage}");
    await sender.SendMessageAsync(new ServiceBusMessage(orderShippedMessage) { SessionId = sessionID }, cancellationToken);
    Console.WriteLine($"Sent: {orderShippedMessage}");
    await sender.SendMessageAsync(new ServiceBusMessage(orderDeliveredMessage) { SessionId = sessionID }, cancellationToken);
    Console.WriteLine($"Sent: {orderDeliveredMessage}");
});

Console.WriteLine();
Console.WriteLine("All messages are sent");
Console.ReadLine();