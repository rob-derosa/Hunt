using Hunt.Common;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
	public class PushService
	{
		static PushService _instance;
		NotificationHubClient _hub;

		public static PushService Instance
		{
			get { return _instance ?? (_instance = new PushService()); }
		}

		public PushService()
		{
			_hub = NotificationHubClient.CreateClientFromConnectionString(Keys.NotificationHub.ConnectionString, Keys.NotificationHub.Name);
		}

		async public Task<string> Register(DeviceRegistration registration)
		{
			const string templateBodyAPNS = "{\"aps\":{\"alert\":\"$(message)\"}}";
			const string templateBodyFCM = "{\"data\":{\"message\":\"$(messageParam)\"}}";

			RegistrationDescription description;

			switch(registration.Platform)
			{
				case "iOS":
					description = new AppleTemplateRegistrationDescription(registration.Handle, templateBodyAPNS, registration.Tags);
					break;

				case "Android":
					description = new GcmTemplateRegistrationDescription(registration.Handle, templateBodyFCM, registration.Tags);
					break;

				default:
					return null;
			}

			var result = await _hub.CreateOrUpdateRegistrationAsync(description);
			return result?.RegistrationId;
		}

		async public Task<bool> SendNotification(string title, string message, string[] tags, Dictionary<string, string> payload = null)
		{
			var outcome = await _hub.SendTemplateNotificationAsync(payload, tags);
			return outcome.Success > 0;
		}
	}
}
