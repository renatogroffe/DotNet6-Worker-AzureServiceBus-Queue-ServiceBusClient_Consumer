using WorkerServiceBusQueue;

Console.WriteLine(
    "Testando o consumo de mensagens com Azure Service Bus + Filas");

if (args.Length != 2)
{
    Console.WriteLine(
        "Informe 2 parametros: " +
        "no primeiro a string de conexao com o Azure Service Bus, " +
        "no segundo a Fila/Queue a ser utilizado no consumo das mensagens...");
    return;
}

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<ExecutionParameters>(
            new ExecutionParameters()
            {
                ConnectionString = args[0],
                Queue = args[1]
            });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();