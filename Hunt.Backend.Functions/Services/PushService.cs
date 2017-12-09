using Hunt.Common;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
	public class PushService
	{
		static PushService _dev;
		static PushService _production;
		NotificationHubClient _hub;

		public static PushService Dev
		{
			get { return _dev ?? (_dev = new PushService(AppMode.Dev)); }
		}

		public static PushService Production
		{
			get { return _production ?? (_production = new PushService(AppMode.Production)); }
		}

		public PushService(AppMode mode)
		{
			var connection = Config.NotificationHub.Production.ConnectionString;
			var name = Config.NotificationHub.Production.Name;

			if(mode == AppMode.Dev)
			{
				connection = Config.NotificationHub.Sandbox.ConnectionString;
				name = Config.NotificationHub.Sandbox.Name;
			}

			_hub = NotificationHubClient.CreateClientFromConnectionString(connection, name);
		}

		async public Task<string> Register(DeviceRegistration registration)
		{
			if (string.IsNullOrWhiteSpace(registration.Handle))
				return null;

			const string templateBodyAPNS = @"{ ""aps"" : { ""content-available"" : 1, ""alert"" : { ""title"" : ""$(title)"", ""body"" : ""$(message)"", ""badge"":""#(badge)"" } }, ""payload"" : ""$(payload)"" }";
			const string templateBodyFCM = @"{""data"":{""title"":""$(title)"",""message"":""$(message)"",""payload"":""$(payload)""}}";

			string newRegistrationId = null;
			// make sure there are no existing registrations for this push handle (used for iOS and Android)
 			var registrations = await _hub.GetRegistrationsByChannelAsync(registration.Handle, 100);

			foreach (var reg in registrations)
				if (newRegistrationId == null)
					newRegistrationId = reg.RegistrationId;
				else
					await _hub.DeleteRegistrationAsync(reg);

			if(newRegistrationId == null)
				newRegistrationId = await _hub.CreateRegistrationIdAsync();

			RegistrationDescription description;

			switch (registration.Platform)
			{
				case "iOS":
					registration.Handle = registration.Handle.Replace("<", "").Replace(">", "").Replace(" ","").ToUpper();
					description = new AppleTemplateRegistrationDescription(registration.Handle, templateBodyAPNS, registration.Tags);
					break;

				case "Android":
					description = new GcmTemplateRegistrationDescription(registration.Handle, templateBodyFCM, registration.Tags);
					break;

				default:
					return null;
			}

			description.RegistrationId = newRegistrationId;
			var result = await _hub.CreateOrUpdateRegistrationAsync(description);
			return result?.RegistrationId;
		}

		async public Task<bool> SendNotification(string message, string[] tags, Dictionary<string, string> payload = null)
		{
			var notification = new Dictionary<string,string> { { "message", message } };
			var json = payload != null ? JsonConvert.SerializeObject(payload) : "";

			notification.Add("title", "Game Update");
			notification.Add("badge", "0");
			notification.Add("payload", "");

			var outcome = await _hub.SendTemplateNotificationAsync(notification, tags);
			return true;
		}

		async public Task<bool> SendSilentNotification(string[] tags, Dictionary<string, string> payload = null)
		{
			var notification = new Dictionary<string, string> { { "message", "" } };
			var json = payload != null ? JsonConvert.SerializeObject(payload) : "";

			notification.Add("title", "");
			notification.Add("badge", "0");
			notification.Add("payload", "");

			var outcome = await _hub.SendTemplateNotificationAsync(notification, tags);
			return true;
		}

	}
}
