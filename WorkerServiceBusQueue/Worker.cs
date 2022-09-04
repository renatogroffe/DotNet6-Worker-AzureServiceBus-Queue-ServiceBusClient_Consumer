using Azure.Messaging.ServiceBus;

namespace WorkerServiceBusQueue;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly ExecutionParameters _executionParameters;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    
    public Worker(ILogger<Worker> logger,
        ExecutionParameters executionParameters)
    {
        _logger = logger;
        _logger.LogInformation(
            $"Queue = {executionParameters.Queue}");

        _executionParameters = executionParameters;
        var clientOptions = new ServiceBusClientOptions()
            { TransportType = ServiceBusTransportType.AmqpWebSockets };
        _client = new ServiceBusClient(
            _executionParameters.ConnectionString, clientOptions);
        _processor = _client.CreateProcessor(
            executionParameters.Queue, new ServiceBusProcessorOptions());
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Iniciando o processamento de mensagens...");
        await _processor.StartProcessingAsync();
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        await _processor.CloseAsync();
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
        _logger.LogInformation(
            "Conexao com o Azure Service Bus fechada!");
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        _logger.LogInformation("[Nova mensagem recebida] " +
            args.Message.Body.ToString());
        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError("[Falha] " +
            args.Exception.GetType().FullName + " " +
            args.Exception.Message);        
        return Task.CompletedTask;
    }
}