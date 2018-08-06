using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace Hunt.Backend.Functions
{
	/// <summary>
	/// Queue service.
	/// </summary>
	public partial class QueueService
	{
        #region Private Properties

		/// <summary>
        /// The queue client
        /// </summary>
		private QueueClient _queueClient;

        /// <summary>
        /// The messaging factory
        /// </summary>
        private MessagingFactory _factory;

        /// <summary>
        /// The queue name
        /// </summary>
        private string _queueName;

		#endregion

		public QueueService(string queueName)
		{
            _queueName = queueName;

            var credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(ConfigManager.Instance.ServiceBusAccessPolicyName,
                ConfigManager.Instance.ServiceBusAccessPolicyKey);

            var serviceBusUri = ServiceBusEnvironment.CreateServiceUri("sb", ConfigManager.Instance.ServiceBusAccountNamespace, string.Empty);

            // first create the messaging factory, then create queue client from endpoint url
            _factory = MessagingFactory.Create(serviceBusUri, credentials);
            _queueClient = _factory.CreateQueueClient(_queueName);
        }

		public async Task SendBrokeredMessageAsync(object item, string gameId = "", 
            string messageLabel = "", int delay = 0)
		{
			try
			{
				var json = JsonConvert.SerializeObject(item);

				var message = new BrokeredMessage(json);
                message.Label = messageLabel;
                message.Properties["DelayMinutes"] = delay;
                message.Properties["gameId"] = gameId;
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMinutes(delay);

                var sender = await _factory.CreateMessageSenderAsync(_queueName);
                await sender.SendAsync(message);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(@"ERROR {0}", e.Message);
			}
		}
	}
}