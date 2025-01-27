﻿using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private const string ConnectionString = "This is my azure service-bus connection string.";
        public async Task PublishMessage(object message, string topic_queue_name)
        {
            try
            {
                await using var client = new ServiceBusClient(ConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });

                var sender = client.CreateSender(topic_queue_name);
                var jsonMsg = JsonConvert.SerializeObject(message);
                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMsg))
                {
                    CorrelationId = Guid.NewGuid().ToString()
                };

                await sender.SendMessageAsync(msg);
                await client.DisposeAsync();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"----- Error in MessageBus: {ex.Message}");
            }

        }
    }
}
