using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Models.Dto;
using Mango.Services.RewardAPI.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly string _serviceBusConnectionString;
        private readonly string _orderCreatedTopic;
        private readonly string _orderCreatedRewardSubscription;
        private readonly IRewardService _rewardService;

        private ServiceBusProcessor _rewardProcessor;
        private ServiceBusProcessor _registerUserProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, IRewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;
            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _orderCreatedTopic = _configuration.GetValue<string>("TopicsAndQueueNames:OrderCreatedTopic");
            _orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicsAndQueueNames:OrderCreated_Reward_Subscription");

            var client = new ServiceBusClient(_serviceBusConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
            _rewardProcessor = client.CreateProcessor(_orderCreatedTopic, _orderCreatedRewardSubscription);
        }

        public async Task Start()
        {
            _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
            _rewardProcessor.ProcessErrorAsync += ErrorHandler;
            await _rewardProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _rewardProcessor.StopProcessingAsync();
            await _rewardProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
           Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var reward = JsonConvert.DeserializeObject<RewardDto>(body);
            try
            {
                await _rewardService.UpdateRewards(reward);
                await args.CompleteMessageAsync(args.Message);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
