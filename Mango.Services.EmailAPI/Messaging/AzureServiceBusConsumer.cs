using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly string _serviceBusConnectionString;
        private readonly string _emailCartQueue;
        private readonly string _registerUserQueue;
        private readonly string _orderCreatedTopic;
        private readonly string _orderCreatedEmailSubscription;
        private readonly IEmailService _emailService;

        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _registerUserProcessor;
        private ServiceBusProcessor _emailOrderPlacedProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _emailCartQueue = _configuration.GetValue<string>("TopicsAndQueueNames:EmailShoppingCartQueue");
            _registerUserQueue = _configuration.GetValue<string>("TopicsAndQueueNames:RegisterUserQueue");
            _orderCreatedTopic = _configuration.GetValue<string>("TopicsAndQueueNames:OrderCreatedTopic");
            _orderCreatedEmailSubscription = _configuration.GetValue<string>("TopicsAndQueueNames:OrderCreated_Email_Subscription");

            var client = new ServiceBusClient(_serviceBusConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
            _emailCartProcessor = client.CreateProcessor(_emailCartQueue);
            _registerUserProcessor = client.CreateProcessor(_registerUserQueue);
            _emailOrderPlacedProcessor = client.CreateProcessor(_orderCreatedTopic, _orderCreatedEmailSubscription);
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();

            _registerUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
            _registerUserProcessor.ProcessErrorAsync += ErrorHandler;
            await _registerUserProcessor.StartProcessingAsync();

            _emailOrderPlacedProcessor.ProcessMessageAsync += OnOrderPlacedRequestReceived;
            _emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailOrderPlacedProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            await _registerUserProcessor.StopProcessingAsync();
            await _registerUserProcessor.DisposeAsync();

            await _emailOrderPlacedProcessor.StopProcessingAsync();
            await _emailOrderPlacedProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
           Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var cart = JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                await _emailService.EmailCartAndLog(cart);
                await args.CompleteMessageAsync(args.Message);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var emailAddress = JsonConvert.DeserializeObject<string>(body);
            try
            {
                await _emailService.RegisterUserEmailAndLog(emailAddress);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task OnOrderPlacedRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var reward = JsonConvert.DeserializeObject<RewardDto>(body);
            try
            {
                await _emailService.LogOrderPlaced(reward);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
