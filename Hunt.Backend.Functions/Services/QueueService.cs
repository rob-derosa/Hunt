using System;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using Hunt.Common;
using System.IO;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace Hunt.Backend.Functions
{
	/// <summary>
	/// Queue service.
	/// </summary>
	public partial class QueueService : IDisposable
	{
        #region Private Properties

        static QueueService defaultInstance = new QueueService("endgame");

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

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Hunt.Mobile.Common.CosmosDataService"/> class.
		/// </summary>
		public QueueService(string queueName)
		{
            _queueName = queueName;

            var credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(Keys.ServiceBus.AccountName,
                Keys.ServiceBus.AccountKey);

            var serviceBusUri = ServiceBusEnvironment.CreateServiceUri("sb", Keys.ServiceBus.AccountNamespace, string.Empty);

            // first create the messaging factory, then create queue client from endpoint url
            _factory = MessagingFactory.Create(serviceBusUri, credentials);
            _queueClient = _factory.CreateQueueClient(_queueName);
        }

		/// <summary>
		/// Inserts the item async.
		/// </summary>
		/// <returns>The item async.</returns>
		/// <param name="item">Item.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
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

		public void Dispose()
		{
		}
	}
}