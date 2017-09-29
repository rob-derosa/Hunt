using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;

namespace Hunt.Mobile.UITest
{
	[TestFixture(Platform.Android)]
	[TestFixture(Platform.iOS)]
	public partial class Tests
	{
		IApp app;
		Platform platform;

		public Tests(Platform platform)
		{
			this.platform = platform;
		}

		[SetUp]
		public void BeforeEachTest()
		{
			app = AppInitializer.StartApp(platform);
		}

		public void Reigster(string email, string firstName)
		{
			app.Screenshot("When the app launches");
			app.WaitForElement("emailEntry");
			app.EnterText("emailEntry", email);

			app.Screenshot("and I enter my email address");
			app.DismissKeyboard();

			app.ClearText("firstNameEntry");
			app.EnterText("firstNameEntry", firstName);
			app.DismissKeyboard();

			app.Screenshot("and I enter my first name");
			app.Tap("continueButton");

			app.Screenshot("and I wait for data to load");
			app.WaitForNoElement("loadingMessage");
		}
	}
}